using UnityEngine;

namespace Vecerdi.CommandPalette.Units.Settings;

public class UnitConversionSettings : ScriptableObject {
    [SerializeField, Range(1, 100), Tooltip("The number of pixels per rem unit (default: 16px).")]
    private float m_RemToPxRatio = 16f;

    public float RemToPxRatio => m_RemToPxRatio;

    internal const string RemToPxRatioProperty = nameof(m_RemToPxRatio);
}