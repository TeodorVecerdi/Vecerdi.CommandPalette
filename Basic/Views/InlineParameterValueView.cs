using System;
using System.Collections.Generic;
using System.Linq;
using FuzzySharp;
using FuzzySharp.SimilarityRatio;
using FuzzySharp.SimilarityRatio.Scorer.StrategySensitive;
using UnityEngine;
using UnityEngine.UIElements;
using Vecerdi.CommandPalette.Basic.Data;
using Vecerdi.CommandPalette.Core;
using Vecerdi.CommandPalette.Utils;
using Vecerdi.CommandPalette.Views;

namespace Vecerdi.CommandPalette.Basic.Views;

public sealed class InlineParameterValueView : View {
    public const float SearchFieldHeight = 70.0f;
    private const float ResultsSpacing = 6.0f;
    private const int MaxDisplayedItemCount = 6;

    public const int MaxItemCount = 100;
    public const float ItemHeight = 64.0f;
    private const int SearchCutoff = 50;

    private string m_SearchString = "";
    private List<InlineParameterResultEntry>? m_SearchResults;
    private List<VisualElement>? m_SearchResultElements;

    private VisualElement m_MainContainer = null!;
    private TextField m_SearchField = null!;
    private ScrollView m_ResultsContainer = null!;
    private VisualElement? m_SelectedElement;
    private int m_SelectedIndex;

    private CommandEntry m_Entry;
    private InlineParameterValues m_InlineParameterResults = null!;

    public void Initialize(CommandEntry entry) {
        m_Entry = entry;
        m_InlineParameterResults = (InlineParameterValues?)entry.Parameters[0].InlineValuesProvider?.Invoke(null, null) ?? new InlineParameterValues();
        foreach (var inlineParameterResult in m_InlineParameterResults) {
            inlineParameterResult.OnSelect += ExecuteEntry;
        }
    }

    public override void OnEvent(Event evt) {
        if (evt is { isKey: true, type: EventType.KeyUp }) {
            if (evt is { shift: true, keyCode: KeyCode.Escape }) {
                Window.Close();
            } else if (evt is { alt: true, keyCode: KeyCode.Backspace }) {
                Window.SwitchToView<MainView>();
            }
        }
    }

    public override VisualElement Build() {
        m_MainContainer = new VisualElement().WithName("MainContainer");
        m_SearchField = new TextField().WithName("SearchField");
        var placeholder = new Label("Start typing...").WithName("SearchPlaceholder").WithClassEnabled("hidden", !string.IsNullOrEmpty(m_SearchString));
        placeholder.pickingMode = PickingMode.Ignore;
        m_SearchField.Add(placeholder);
        m_SearchField.RegisterValueChangedCallback(evt => {
            m_SearchString = evt.newValue;
            placeholder.EnableInClassList("hidden", !string.IsNullOrEmpty(m_SearchString));
            UpdateResults();
        });

        EventCallback<KeyDownEvent, InlineParameterValueView> keyDownCallback = static (evt, view) => {
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
                    view.m_MainContainer.schedule.Execute(() => { view.m_SearchField.hierarchy[0].Focus(); });
                    return;
                }

                if (view.m_SelectedElement.userData is InlineParameterResultEntry entry) {
                    view.ExecuteEntry(entry);
                }
            } else if (evt.altKey && evt.keyCode == KeyCode.Backspace) {
                evt.StopPropagation();
                evt.StopImmediatePropagation();
                view.Window.SwitchToView<MainView>();
            }
        };

#if UNITY_6000_0_OR_NEWER
        m_SearchField.Q<TextElement>().RegisterCallback(keyDownCallback, this);
#else
        m_SearchField.RegisterCallback(keyDownCallback, this);
#endif

        m_MainContainer.Add(m_SearchField);

        m_ResultsContainer = new ScrollView(ScrollViewMode.Vertical).WithName("ResultsContainer");
        m_MainContainer.Add(m_ResultsContainer);
        m_SearchField.value = m_SearchString;

        m_MainContainer.schedule.Execute(() => {
            m_SearchField.hierarchy[0].Focus();
            UpdateResults();
        });

        return m_MainContainer;
    }

    private void UpdateResults() {
        m_SearchResults = GetSearchResults(m_SearchString);
        UpdateResultsView();
    }

    private List<InlineParameterResultEntry> GetSearchResults(string query) {
        if (string.IsNullOrWhiteSpace(query)) {
            return m_InlineParameterResults;
        }

        var results =
            CommandPaletteScorer.ScoreResults(
                new InlineParameterResultEntry(null, new ResultDisplaySettings(query)),
                SearchCutoff,
                m_InlineParameterResults,
                entry => entry.DisplaySettings.Title
            );

        var entries = results.Select(result => result.Value).ToList();
        if (entries.Count == 0) {
            entries = Process.ExtractSorted(
                new InlineParameterResultEntry(null, new ResultDisplaySettings(query)),
                m_InlineParameterResults, entry => entry.DisplaySettings.Title, ScorerCache.Get<PartialRatioScorer>()).Select(result => result.Value).ToList();
        }

        return entries;
    }

    private void UpdateResultsView() {
        m_ResultsContainer.Clear();
        m_SelectedElement = null;
        if (m_SearchResults == null || m_SearchResults.Count == 0) {
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
            m_ResultsContainer.AddToClassList("hidden");
            Window.SetHeight(SearchFieldHeight);
            return;
        }

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

        if (m_SelectedElement is not null) {
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

        if (m_SelectedElement is not null) {
            m_ResultsContainer.ScrollTo(m_SelectedElement);
        }
    }

    private void ExecuteEntry(InlineParameterResultEntry entry) {
        m_Entry.Method.Invoke(null, new[] { entry.Value });
        Window.Close();
    }
}
