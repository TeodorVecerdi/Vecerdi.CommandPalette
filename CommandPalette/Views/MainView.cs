using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Vecerdi.CommandPalette.Core;
using Vecerdi.CommandPalette.PluginSupport;
using Vecerdi.CommandPalette.Settings;
using Vecerdi.CommandPalette.Utils;

namespace Vecerdi.CommandPalette.Views;

public sealed class MainView : View {
    private const float ResultsSpacing = 6.0f;
    private const int MaxDisplayedItemCount = 6;

    public const float SearchFieldHeight = 70.0f;
    public const int MaxItemCount = 100;
    public const float ItemHeight = 64.0f;

    private static string s_SearchString = "";
    private static CommandPaletteSettings s_Settings = null!;

    private List<ResultEntry>? m_SearchResults;
    private List<VisualElement>? m_SearchResultElements;

    private VisualElement m_MainContainer = null!;
    private TextField m_SearchField = null!;
    private ScrollView m_ResultsContainer = null!;
    private VisualElement? m_SelectedElement;
    private Label m_NoResultsLabel = null!;
    private int m_SelectedIndex;

    public override void OnEvent(Event evt) {
        if (evt.isKey && evt.type == EventType.KeyUp) {
            if (evt.shift && evt.keyCode == KeyCode.Escape) {
                Window.Close();
            } else if (evt.alt && evt.keyCode == KeyCode.Backspace) {
                Window.Close();
            }
        }
    }

    public override VisualElement Build() {
        if (s_Settings == null) {
            s_Settings = CommandPaletteSettings.GetOrCreateSettings();
        }

        m_MainContainer = new VisualElement().WithName("MainContainer");
        m_SearchField = new TextField().WithName("SearchField");
        var placeholder = new Label("Start typing...").WithName("SearchPlaceholder").WithClassEnabled("hidden", !string.IsNullOrEmpty(s_SearchString));
        placeholder.pickingMode = PickingMode.Ignore;
        m_SearchField.Add(placeholder);
        m_SearchField.RegisterValueChangedCallback(evt => {
            s_SearchString = evt.newValue;
            placeholder.EnableInClassList("hidden", !string.IsNullOrEmpty(s_SearchString));
            UpdateResults();
        });

        EventCallback<KeyDownEvent, MainView> keyDownCallback = static (evt, view) => {
            if (evt.keyCode == KeyCode.Escape) {
                view.Window.Close();
            } else if (evt.keyCode == KeyCode.DownArrow) {
                view.SelectNext();
                evt.StopPropagation();
            } else if (evt.keyCode == KeyCode.UpArrow) {
                view.SelectPrevious();
                evt.StopPropagation();
            } else if (evt.keyCode == KeyCode.Return) {
                if (view.m_SelectedElement == null) {
                    evt.StopPropagation();
                    view.m_MainContainer.schedule.Execute(() => view.m_SearchField.hierarchy[0].Focus());
                    return;
                }

                if (view.m_SelectedElement?.userData is ResultEntry entry) {
                    view.ExecuteEntry(entry);
                }
            }
        };

#if UNITY_6000_0_OR_NEWER
        m_SearchField.Q<TextElement>().RegisterCallback(keyDownCallback, this);
#else
        m_SearchField.RegisterCallback(keyDownCallback, this);
#endif

        m_MainContainer.Add(m_SearchField);

        m_NoResultsLabel = new Label("No Results Found").WithName("NoResultsLabel").WithClasses("hidden");
        m_MainContainer.Add(m_NoResultsLabel);

        m_ResultsContainer = new ScrollView(ScrollViewMode.Vertical).WithName("ResultsContainer");
        m_MainContainer.Add(m_ResultsContainer);
        m_SearchField.value = s_SearchString;

        m_MainContainer.schedule.Execute(() => {
            m_SearchField.hierarchy[0].Focus();
            UpdateResults();
        });

        return m_MainContainer;
    }

    private void UpdateResults() {
        m_SearchResults = PluginManager.GetResultsSorted(new Query(s_SearchString));
        UpdateResultsView();
    }

    private void UpdateResultsView() {
        m_ResultsContainer.Clear();
        m_SelectedElement = null;
        if (m_SearchResults == null || m_SearchResults.Count == 0) {
            m_NoResultsLabel.RemoveFromClassList("hidden");
            m_ResultsContainer.AddToClassList("hidden");
            Window.SetHeight(SearchFieldHeight);
            return;
        }

        var entries = m_SearchResults;
        if (entries.Count > 0) {
            var displayedCount = Math.Min(entries.Count, MaxDisplayedItemCount);
            var extraHeight = displayedCount * ItemHeight + (displayedCount + 1) * ResultsSpacing;
            Window.SetHeight(SearchFieldHeight + extraHeight);
        }

        if (entries.Count == 0) {
            m_NoResultsLabel.RemoveFromClassList("hidden");
            m_ResultsContainer.AddToClassList("hidden");
            Window.SetHeight(SearchFieldHeight);
            return;
        }

        m_NoResultsLabel.AddToClassList("hidden");
        m_ResultsContainer.RemoveFromClassList("hidden");
        m_ResultsContainer.style.paddingTop = ResultsSpacing;
        m_ResultsContainer.style.paddingBottom = ResultsSpacing;
        m_SearchResultElements = new List<VisualElement>();

        var index = 0;
        foreach (var entry in entries) {
            var resultElement = CommandPaletteUtility.CreateEntryElement(entry);
            resultElement.AddManipulator(new Clickable(() => { ExecuteEntry(entry); }));

            if (index == 0) {
                resultElement.AddToClassList("selected");
                m_SelectedElement = resultElement;
                m_SelectedIndex = 0;
            } else {
                resultElement.style.marginTop = ResultsSpacing;
            }

            m_SearchResultElements.Add(resultElement);
            m_ResultsContainer.Add(resultElement);

            index++;
            if (index >= MaxItemCount) {
                break;
            }
        }
    }

    private void SelectPrevious() {
        if (m_SearchResultElements == null || m_SearchResultElements.Count == 0) {
            return;
        }

        if (m_SelectedIndex != 0) {
            m_SelectedIndex--;
        } else {
            m_SelectedIndex = m_SearchResultElements.Count - 1;
        }

        m_SelectedElement?.RemoveFromClassList("selected");
        m_SelectedElement = m_SearchResultElements[m_SelectedIndex];
        m_SelectedElement?.AddToClassList("selected");
        if (m_SelectedElement != null && m_ResultsContainer.Contains(m_SelectedElement)) {
            m_ResultsContainer.ScrollTo(m_SelectedElement);
        }
    }

    private void SelectNext() {
        if (m_SearchResultElements == null || m_SearchResultElements.Count == 0) {
            return;
        }

        if (m_SelectedIndex != m_SearchResultElements.Count - 1) {
            m_SelectedIndex++;
        } else {
            m_SelectedIndex = 0;
        }

        m_SelectedElement?.RemoveFromClassList("selected");
        m_SelectedElement = m_SearchResultElements[m_SelectedIndex];
        m_SelectedElement?.AddToClassList("selected");
        if (m_SelectedElement != null && m_ResultsContainer.Contains(m_SelectedElement)) {
            m_ResultsContainer.ScrollTo(m_SelectedElement);
        }
    }

    private void ExecuteEntry(ResultEntry entry) {
        if (entry.OnSelect?.Invoke(entry) ?? false) {
            if (s_Settings.ClearSearchOnSelection) {
                s_SearchString = "";
            }

            Window.Close();
        }
    }
}
