
namespace Crest
{
    using System;
    using UnityEditor;

    public abstract class ValidatedInspectorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            IValidated target = (IValidated)this.target;

            var messageTypes = Enum.GetValues(typeof(MessageType));

            // This is a static list. Not sure if this will ever be a threaded operation which would be an issue.
            ValidatedHelper.messages.Clear();
            // We will need to handle the null case for this
            var ocean = FindObjectOfType<OceanRenderer>();
            target.Validate(ocean, ValidatedHelper.HelpBox);

            // We only want space before and after the list of help boxes. We don't want space between.
            var needsSpaceAbove = true;
            var needsSpaceBelow = false;

            // We loop through in reverse order so Error appears at the top
            for (var messageTypeIndex = messageTypes.Length - 1; messageTypeIndex >= 0; messageTypeIndex--)
            {
                // We would want make sure we don't produce garbage here
                var filtered = ValidatedHelper.messages.FindAll(x => (int) x.type == messageTypeIndex);
                if (filtered.Count > 0)
                {
                    if (needsSpaceAbove)
                    {
                        // Double space looks correct at top.
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        needsSpaceAbove = false;
                    }

                    needsSpaceBelow = true;

                    // We join the messages together to reduce vertical space since HelpBox has padding and borders etc.
                    var joinedMessages = "";
                    for (var ii = 0; ii < filtered.Count; ii++)
                    {
                        // We would want to reduce garbage here using string builder.
                        joinedMessages += filtered[ii].message;
                        if (ii < filtered.Count - 1) joinedMessages += "\n";
                    }

                    EditorGUILayout.HelpBox(joinedMessages, (MessageType)messageTypeIndex);

                    // We could conditionally break here to hide less important messages like MessageType.Info
                    // break;
                }
            }

            if (needsSpaceBelow)
            {
                EditorGUILayout.Space();
            }

            base.OnInspectorGUI();
        }
    }

    // This is how we target inspectors. We would have to create one for each one we are targetting
    // We can use inheritence here so that will save a fair bit of definitions.
    [CustomEditor(typeof(RegisterLodDataInputBase), true), CanEditMultipleObjects]
    class RegisterLodDataInputBaseValidatedInspectorEditor : ValidatedInspectorEditor {}
}
