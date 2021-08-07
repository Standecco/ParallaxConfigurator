using System;
using System.Globalization;
using UnityEngine;

namespace ParallaxConfigurator
{
    public class InputFields
    {
        private static int activeFieldID = -1;

        private static float  activeFloatFieldLastValue = 0;
        private static string activeFloatFieldString    = "";

        private static Color  activeColorFieldLastValue = new Color();
        private static string activeColorFieldString    = "";

        private static string activeTexFieldLastValue = "";
        private static string activeTexFieldString    = "";

        private static readonly GUIStyle TexStyle         = new GUIStyle(GUI.skin.textArea) { wordWrap = true };
        private static readonly GUIStyle ResetButtonStyle = new GUIStyle(GUI.skin.button) { fontSize = 9 };
        private static readonly GUIStyle FloatStyle       = new GUIStyle(GUI.skin.textArea);
        private static readonly GUIStyle ColorStyle       = new GUIStyle(GUI.skin.textArea);

        /// <summary>
        /// Float input field for in-game cfg editing. Behaves exactly like UnityEditor.EditorGUILayout.FloatField
        /// </summary>
        public static float FloatField(float value)
        {
            // Get rect and control for this float field for identification
            Rect pos = GUILayoutUtility.GetRect(new GUIContent(value.ToString(CultureInfo.InvariantCulture)),
                GUI.skin.label, GUILayout.ExpandWidth(false), GUILayout.MinWidth(50));
            int floatFieldID = GUIUtility.GetControlID("FloatField".GetHashCode(), FocusType.Keyboard, pos) + 1;
            if (floatFieldID == 0)
                return value;

            // has the value been recorded?
            bool recorded = activeFieldID == floatFieldID;
            // is the field being edited?
            bool active = floatFieldID == GUIUtility.keyboardControl;

            if (active && recorded && activeFloatFieldLastValue != value)
            {
                // Value has been modified externally
                activeFloatFieldLastValue = value;
                activeFloatFieldString = value.ToString(CultureInfo.InvariantCulture);
            }

            // Get stored string for the text field if this one is recorded
            string str = recorded ? activeFloatFieldString : value.ToString(CultureInfo.InvariantCulture);

            // pass it in the text field
            string strValue = GUI.TextField(pos, str, FloatStyle);

            // Update stored value if this one is recorded
            if (recorded)
                activeFloatFieldString = strValue;

            // Try Parse if value got changed. If the string could not be parsed, ignore it and keep last value
            bool parsed = true;
            if (strValue != value.ToString(CultureInfo.InvariantCulture))
            {
                parsed = float.TryParse(strValue, out float newValue);
                if (parsed)
                    value = activeFloatFieldLastValue = newValue;
            }

            if (active && !recorded)
            {
                // Gained focus this frame
                activeFieldID = floatFieldID;
                activeFloatFieldString = strValue;
                activeFloatFieldLastValue = value;
            }
            else if (!active && recorded)
            {
                // Lost focus this frame
                activeFieldID = -1;
                if (!parsed)
                    value = strValue.ForceParseFloat();
            }

            return value;
        }

        /// <summary>
        /// Color input field for in-game cfg editing. Parses basic colors (e.g. "yellow"), as well as any
        /// string in the format "r, g, b, (a)". Ignores any character before or after the comma-separated floats.
        /// </summary>
        public static Color ColorField(Color value)
        {
            // Get rect and control for this float field for identification
            Rect pos = GUILayoutUtility.GetRect(new GUIContent(value.ToString()), GUI.skin.label,
                GUILayout.ExpandWidth(false), GUILayout.MinWidth(220));
            int colorFieldID = GUIUtility.GetControlID("ColorField".GetHashCode(), FocusType.Keyboard, pos) + 1;
            if (colorFieldID == 0)
                return value;

            // has the value been recorded?
            bool recorded = activeFieldID == colorFieldID;
            // is the field being edited?
            bool active = colorFieldID == GUIUtility.keyboardControl;

            if (active && recorded && activeColorFieldLastValue != value)
            {
                // Value has been modified externally
                activeColorFieldLastValue = value;
                activeColorFieldString = value.ToString();
            }

            // Get stored string for the text field if this one is recorded
            string str = recorded ? activeColorFieldString : value.ToString();

            // pass it in the text field
            string strValue = GUI.TextField(pos, str, ColorStyle);

            // Update stored value if this one is recorded
            if (recorded)
                activeColorFieldString = strValue;

            // Try Parse if value got changed. If the string could not be parsed, ignore it and keep last value
            bool parsed = true;
            if (strValue != value.ToString())
            {
                parsed = ColorUtility.TryParseHtmlString(strValue, out Color newValue);
                if (parsed)
                    value = activeColorFieldLastValue = newValue;
            }

            if (active && !recorded)
            {
                // Gained focus this frame
                activeFieldID = colorFieldID;
                activeColorFieldString = strValue;
                activeColorFieldLastValue = value;
            }
            else if (!active && recorded)
            {
                // Lost focus this frame
                activeFieldID = -1;
                if (!parsed)
                    value = strValue.ForceParseColor(activeColorFieldLastValue);
            }

            return value;
        }

        /// <summary>
        /// Special-purpose Text input field for in-game editing. Meant for dds or png textures' paths.
        /// Check if texture file exists before applying the value.
        /// </summary>
        public static string TexField(string value)
        {
            // Get rect and control for this float field for identification
            Rect pos = GUILayoutUtility.GetRect(new GUIContent(value), GUI.skin.label, GUILayout.ExpandWidth(false),
                GUILayout.MinWidth(200));
            int texFieldID = GUIUtility.GetControlID("TexField".GetHashCode(), FocusType.Keyboard, pos) + 1;
            if (texFieldID == 0)
                return value;

            // has the value been recorded?
            bool recorded = activeFieldID == texFieldID;
            // is the field being edited?
            bool active = texFieldID == GUIUtility.keyboardControl;

            if (active && recorded && activeTexFieldLastValue != value)
            {
                // Value has been modified externally
                activeTexFieldLastValue = value;
                activeTexFieldString = string.Copy(value);
            }

            // Get stored string for the text field if this one is recorded
            string str = recorded ? activeTexFieldString : string.Copy(value);

            // pass it in the text field
            string strValue = GUI.TextField(pos, str, TexStyle);

            // Update stored value if this one is recorded
            if (recorded)
                activeTexFieldString = strValue;

            // Try Parse if value got changed. If the string could not be parsed, ignore it and keep last value
            bool valid = true;
            if (strValue != value)
            {
                string path = KSPUtil.ApplicationRootPath + "GameData/" + strValue;

                if (!System.IO.File.Exists(path))
                    valid = false;

                if (!path.EndsWith(".dds", StringComparison.OrdinalIgnoreCase) &&
                    !path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    valid = false;

                if (valid)
                    value = activeTexFieldLastValue = strValue;
            }

            if (active && !recorded)
            {
                // Gained focus this frame
                activeFieldID = texFieldID;
                activeTexFieldString = strValue;
                activeTexFieldLastValue = value;
            }
            else if (!active && recorded)
            {
                // Lost focus this frame
                activeFieldID = -1;
                if (!valid)
                    value = activeTexFieldLastValue;
            }

            return value;
        }

        /// <summary>
        /// Float input field with label. Displays the label on the left, and the input field on the right.
        /// </summary>
        public static float FloatField(string label, float value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label + " [float] ", GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();

            value = FloatField(value);

            if (GUILayout.Button("Undo", ResetButtonStyle))
                value = (float) Utils.GetVariableOriginalValue(Utils.VarFromLabels[label]);

            GUILayout.EndHorizontal();
            return value;
        }

        /// <summary>
        /// Int input field with label. Displays the label on the left, and the input field on the right.
        /// Just a FloatField, but with returned value being rounded to nearest int.
        /// </summary>
        public static int IntField(string label, float value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label + " [float] ", GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();

            value = FloatField(value);

            GUILayout.EndHorizontal();
            return Mathf.RoundToInt(value);
        }

        /// <summary>
        /// Color input field with label. Displays the label on the left, and the input field on the right.
        /// </summary>
        public static Color ColorField(string label, Color value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label + " [Color] ", GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();

            value = ColorField(value);

            if (GUILayout.Button("Undo", ResetButtonStyle))
                value = (Color) Utils.GetVariableOriginalValue(Utils.VarFromLabels[label]);

            GUILayout.EndHorizontal();
            return value;
        }

        /// <summary>
        /// Texture path input field with label. Displays the label on the left, and the input field on the right.
        /// </summary>
        public static string TexField(string label, string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label + " [Texture] ", GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();

            value = TexField(value);

            if (GUILayout.Button("Undo", ResetButtonStyle))
                value = (string) Utils.GetVariableOriginalValue(Utils.VarFromLabels[label]);

            GUILayout.EndHorizontal();
            return value;
        }
    }
}