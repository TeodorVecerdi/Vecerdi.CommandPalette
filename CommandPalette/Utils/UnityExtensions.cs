using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Vecerdi.CommandPalette.Utils;

public static class UnityExtensions {
    public static Type[] GetAllDerivedTypes(this AppDomain aAppDomain, Type aType) {
        List<Type> result = [];
        Assembly[] assemblies = aAppDomain.GetAssemblies();
        foreach (var assembly in assemblies) {
            var types = assembly.GetTypes();
            foreach (var type in types)
                if (type.IsSubclassOf(aType))
                    result.Add(type);
        }

        return result.ToArray();
    }

    private static Type? s_ContainerWinType;
    private static FieldInfo s_ShowModeField = null!;
    private static PropertyInfo s_PositionProperty = null!;

    public static Object GetEditorMainWindow() {
        if (s_ContainerWinType == null) {
            s_ContainerWinType = AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(ScriptableObject)).FirstOrDefault(t => t.Name == "ContainerWindow");
            if (s_ContainerWinType == null)
                throw new MissingMemberException("Can't find internal type ContainerWindow. Maybe something has changed inside Unity");
            s_ShowModeField = s_ContainerWinType.GetField("m_ShowMode", BindingFlags.NonPublic | BindingFlags.Instance)!;
            s_PositionProperty = s_ContainerWinType.GetProperty("position", BindingFlags.Public | BindingFlags.Instance)!;
            if (s_ShowModeField == null || s_PositionProperty == null)
                throw new MissingFieldException("Can't find internal fields 'm_ShowMode' or 'position'. Maybe something has changed inside Unity");
        }

        Object[] windows = Resources.FindObjectsOfTypeAll(s_ContainerWinType);
        foreach (var win in windows) {
            var showmode = (int)s_ShowModeField.GetValue(win);
            if (showmode == 4) {
                return win;
            }
        }

        throw new NotSupportedException("Can't find internal main window. Maybe something has changed inside Unity");
    }

    public static Rect GetEditorMainWindowPos(Object window) {
        if (window == null) {
            throw new ArgumentNullException(nameof(window));
        }

        return (Rect)s_PositionProperty.GetValue(window, null);
    }
}
