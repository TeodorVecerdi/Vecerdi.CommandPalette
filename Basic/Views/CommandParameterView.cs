using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Vecerdi.CommandPalette.Basic.Data;
using Vecerdi.CommandPalette.Basic.Drivers;
using Vecerdi.CommandPalette.Utils;
using Vecerdi.CommandPalette.Views;

namespace Vecerdi.CommandPalette.Basic.Views;

public class CommandParameterView : View {
    private const float ParameterTitleHeight = 64.0f;
    private const float ParameterTitleSpacing = 6.0f;
    private const float ParameterPadding = 16.0f;
    private const float ParameterSpacing = 6.0f;
    private const float ParameterHeight = 48.0f;
    private const float ParameterExecuteButtonHeight = 32.0f;
    private const int MaxDisplayedParameterCount = 6;

    private VisualElement m_ParametersContainer = null!;
    private CommandEntry m_Entry;

    public CommandEntry Entry {
        get => m_Entry;
        set => m_Entry = value;
    }

    public override void OnEvent(Event evt) {
        if (evt is { alt: true, keyCode: KeyCode.Backspace }) {
            Window.SwitchToView<MainView>();
        }
    }

    public override VisualElement Build() {
        m_ParametersContainer = new VisualElement().WithName("ParametersContainer");
        LoadParameters();
        return m_ParametersContainer;
    }

    private void LoadParameters() {
        m_ParametersContainer.Clear();
        var titleContainer = new VisualElement().WithClasses("result-entry-main-container", "parameter-title");
        titleContainer.Add(
            new VisualElement().WithClasses("result-entry-title-container", "parameter-title-title-container").WithChildren(
                new Label(m_Entry.ShortName).WithClasses("result-entry-short", "parameter-title-short"),
                new Label($"{m_Entry.DisplayName}").WithClasses("result-entry-display", "parameter-title-display")
            )
        );
        if (!string.IsNullOrWhiteSpace(m_Entry.Description)) {
            titleContainer.Add(new Label(m_Entry.Description).WithClasses("result-entry-description", "parameter-title-description"));
            titleContainer.AddToClassList("has-description");
        }

        titleContainer.style.height = ParameterTitleHeight;
        titleContainer.style.marginBottom = ParameterTitleSpacing;
        m_ParametersContainer.Add(titleContainer);

        CommandParameterValues parameterValues = new(m_Entry.Parameters);
        m_ParametersContainer.userData = new object[] { m_Entry, parameterValues };

        m_ParametersContainer.style.paddingTop = ParameterPadding;
        m_ParametersContainer.style.paddingBottom = ParameterPadding;
        m_ParametersContainer.style.paddingLeft = ParameterPadding;
        m_ParametersContainer.style.paddingRight = ParameterPadding;

        var displayedParameters = Mathf.Min(MaxDisplayedParameterCount, m_Entry.Parameters.Count(parameter => CommandPaletteParameterDriver.IsKnownType(parameter.Type)));
        var height = displayedParameters * ParameterHeight + displayedParameters * ParameterSpacing + ParameterTitleHeight + ParameterTitleSpacing + ParameterExecuteButtonHeight + 2.0f * ParameterPadding;

        Window.SetHeight(height);

        CreateParameterFields(parameterValues);

        m_ParametersContainer.Add(new Button(() => {
            m_Entry.Method.Invoke(null, parameterValues.Values);
            Window.Close();
        }).Initialized(button => {
            button.style.marginTop = ParameterSpacing;
            button.style.height = ParameterExecuteButtonHeight;
        }).WithText("Execute").WithName("ExecuteEntryWithParameters"));
    }

    private void CreateParameterFields(CommandParameterValues parameterValues) {
        var scrollView = new ScrollView().WithClasses("parameters-scroll-view");
        m_ParametersContainer.Add(scrollView);

        var displayedParameters = Mathf.Min(MaxDisplayedParameterCount, parameterValues.Parameters.Count(parameter => CommandPaletteParameterDriver.IsKnownType(parameter.Type)));
        var height = displayedParameters * ParameterHeight + displayedParameters * ParameterSpacing;
        scrollView.style.height = height;

        VisualElement? firstField = null;
        for (var i = 0; i < parameterValues.Values.Length; i++) {
            if (!CommandPaletteParameterDriver.IsKnownType(parameterValues.Parameters[i].Type)) {
                continue;
            }

            var parameterField = CommandPaletteParameterDriver.CreateParameterField(parameterValues.Parameters[i].Type, parameterValues, i);
            if (i == 0) {
                firstField = parameterField;
            } else {
                parameterField.style.marginTop = ParameterSpacing;
            }

            parameterField.style.height = ParameterHeight;

            parameterField.RegisterCallback<KeyDownEvent, CommandParameterView>(static (evt, view) => {
                if (evt.keyCode == KeyCode.Return && evt.altKey) {
                    evt.StopImmediatePropagation();

                    if (view.m_ParametersContainer.userData is object[] userData) {
                        var entry = (CommandEntry)userData[0];
                        entry.Method.Invoke(null, ((CommandParameterValues)userData[1]).Values);
                        view.Window.Close();
                    }
                }
            }, this);

            scrollView.Add(parameterField);
        }

        m_ParametersContainer.schedule.Execute(() => { firstField?.Focus(); });
    }
}
