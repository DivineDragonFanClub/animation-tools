using UnityEngine;
using UnityEngine.UIElements;

namespace DivineDragon
{
    public static class TextHighlightUtility
    {
        /// <summary>
        /// Creates highlighted text labels within a container based on search matches.
        /// </summary>
        /// <param name="container">The container to add the highlighted text to</param>
        /// <param name="text">The text to display</param>
        /// <param name="searchTerm">The search term to highlight</param>
        /// <param name="highlightColor">Optional custom highlight color (defaults to yellow)</param>
        public static void CreateHighlightedText(VisualElement container, string text, string searchTerm, Color? highlightColor = null)
        {
            container.Clear();
            
            if (string.IsNullOrEmpty(searchTerm) || string.IsNullOrEmpty(text))
            {
                var label = CreateStyledLabel(text);
                container.Add(label);
                return;
            }
            
            var textLower = text.ToLower();
            var searchLower = searchTerm.ToLower();
            var lastIndex = 0;
            var defaultHighlightColor = highlightColor ?? new Color(1f, 0.8f, 0.2f, 0.3f);
            
            while (true)
            {
                var matchIndex = textLower.IndexOf(searchLower, lastIndex);
                if (matchIndex < 0)
                {
                    if (lastIndex < text.Length)
                    {
                        var remainingLabel = CreateStyledLabel(text.Substring(lastIndex));
                        container.Add(remainingLabel);
                    }
                    break;
                }
                
                if (matchIndex > lastIndex)
                {
                    var beforeLabel = CreateStyledLabel(text.Substring(lastIndex, matchIndex - lastIndex));
                    container.Add(beforeLabel);
                }
                
                var matchLabel = CreateStyledLabel(text.Substring(matchIndex, searchTerm.Length));
                matchLabel.style.backgroundColor = defaultHighlightColor;
                matchLabel.style.color = Color.white;
                container.Add(matchLabel);
                
                lastIndex = matchIndex + searchTerm.Length;
            }
        }
        
        /// <summary>
        /// Creates a properly styled label for text highlighting
        /// </summary>
        private static Label CreateStyledLabel(string text)
        {
            var label = new Label(text);
            label.style.marginLeft = 0;
            label.style.marginRight = 0;
            label.style.marginTop = 0;
            label.style.marginBottom = 0;
            label.style.paddingLeft = 0;
            label.style.paddingRight = 0;
            label.style.paddingTop = 0;
            label.style.paddingBottom = 0;
            label.style.flexShrink = 0;
            label.style.flexGrow = 0;
            label.focusable = false;
            return label;
        }
    }
}