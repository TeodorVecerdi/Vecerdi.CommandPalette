using UnityEngine;

namespace Vecerdi.CommandPalette.Basic.Settings;

public class CommandsPluginSettings : ScriptableObject {
    [SerializeField, Range(0, 100)] private int m_SearchCutoff = 80;

    public int SearchCutoff => m_SearchCutoff;

    internal const string SearchCutoffProperty = nameof(m_SearchCutoff);
}
