using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui.CompletionWindow;

namespace MagickScriptEditor
{
    public class CompletionDataProvider : ICompletionDataProvider
    {
        private ImageList imageList = new ImageList();

        private List<DefaultCompletionData> completionData = new List<DefaultCompletionData>();

        public CompletionDataProvider()
        {
            List<String> keywords = new List<String>();
            XmlDocument doc = new XmlDocument();
            doc.Load(@"MagickScript.xsd");
            XmlNamespaceManager manager = new XmlNamespaceManager(doc.NameTable);
            manager.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");
            XmlNodeList nodes = doc.SelectNodes(@"//xs:group[@name='actions']//@name", manager);
            Debug.Assert(nodes != null, "nodes != null");
            foreach (XmlNode node in nodes)
            {
                String keyword;
                if ((!String.IsNullOrEmpty(keyword = node.Value) && !keywords.Contains(keyword)))
                {
                    keywords.Add(keyword);
                }
            }
            foreach (String keyword in keywords)
            {
                completionData.Add(new DefaultCompletionData(keyword, 0));
            }
        }

        public ImageList ImageList
        {
            get { return imageList; }
        }

        public string PreSelection
        {
            get { return null; }
        }

        public int DefaultIndex
        {
            get { return 0; }
        }

        public CompletionDataProviderKeyResult ProcessKey(char key)
        {
            if (char.IsLetterOrDigit(key))
            {
                return CompletionDataProviderKeyResult.NormalKey;
            }
            return CompletionDataProviderKeyResult.InsertionKey;
        }

        public bool InsertAction(ICompletionData data, TextArea textArea, int insertionOffset, char key)
        {
            textArea.Caret.Position = textArea.Document.OffsetToPosition(insertionOffset);
            return data.InsertAction(textArea, key);
        }

        public ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped)
        {
            return completionData.ToArray();
        }
    }
}