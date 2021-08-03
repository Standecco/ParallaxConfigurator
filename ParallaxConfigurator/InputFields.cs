using UnityEngine;
using System.Globalization;
using System.Collections.Generic;


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

        private static readonly GUIStyle TexStyle = new GUIStyle(GUI.skin.textArea) {wordWrap = true};

        /// <summary>
        /// Float Field for in-game purposes. Behaves exactly like UnityEditor.EditorGUILayout.FloatField
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
            string strValue = GUI.TextField(pos, str);

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
            string strValue = GUI.TextField(pos, str);

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
                activeTexFieldLastValue = string.Copy(value);
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
                valid = GameDatabase.Instance.ExistsTexture(strValue);
                if (valid)
                    value = activeTexFieldLastValue = strValue;
            }

            if (active && !recorded)
            {
                // Gained focus this frame
                activeFieldID = texFieldID;
                activeTexFieldString = strValue;
                activeTexFieldLastValue = string.Copy(value);
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
        /// Float Field for in-game purposes. Behaves exactly like UnityEditor.EditorGUILayout.FloatField
        /// </summary>
        public static float FloatField(GUIContent label, float value, GUIContent labelType = null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, label != GUIContent.none ? GUILayout.ExpandWidth(true) : GUILayout.ExpandWidth(false));
            if (labelType != null)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(labelType, labelType != GUIContent.none ? GUILayout.ExpandWidth(true) : GUILayout.ExpandWidth(false));
            }
            value = FloatField(value);
            GUILayout.EndHorizontal();
            return value;
        }
        
        public static Color ColorField(GUIContent label, Color value, GUIContent labelType = null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, label != GUIContent.none ? GUILayout.ExpandWidth(true) : GUILayout.ExpandWidth(false));
            if (labelType != null)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(labelType, labelType != GUIContent.none ? GUILayout.ExpandWidth(true) : GUILayout.ExpandWidth(false));
            }
            value = ColorField(value);
            GUILayout.EndHorizontal();
            return value;
        }
        
        public static string TexField(GUIContent label, string value, GUIContent labelType = null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, label != GUIContent.none ? GUILayout.ExpandWidth(true) : GUILayout.ExpandWidth(false));
            if (labelType != null)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(labelType, labelType != GUIContent.none ? GUILayout.ExpandWidth(true) : GUILayout.ExpandWidth(false));
            }
            value = TexField(value);
            GUILayout.EndHorizontal();
            return value;
        }
    }
}