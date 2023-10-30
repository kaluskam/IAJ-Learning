using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.IAJ.Unity.DecisionMaking.RL;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Utils
{
    public static class QTablePrinter
    {
        static int STATE_CELL_LEN = 150;
        static int ACTION_CELL_LEN = 40;
        public static string Pad(string s, int maxLength, char sign)
        {
            StringBuilder sb = new StringBuilder(s);
            while (sb.Length < maxLength) 
            {
                sb.Append(sign);
            }
            return sb.ToString();
        }

        private static string CreateActionsHeader(Action[] actions)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Pad(sb.ToString(), STATE_CELL_LEN, ' ') + '|');
            foreach(Action action in actions)
            {
                sb.Append(QTablePrinter.Pad(action.Name, ACTION_CELL_LEN, ' ') + '|');
            }
            sb.Append("\n");
            return sb.ToString();
        }

        public static string CreatePrintableRepresentantionOfQTable(QTable qTable)
        {
            StringBuilder sb = new StringBuilder();
            RLState[] states = qTable.GetStates();
            Action[] actions = qTable.GetActions();

            sb.Append(QTablePrinter.CreateActionsHeader(actions));
            foreach (var state in states)
            {
                StringBuilder line = new StringBuilder();
                line.Append(QTablePrinter.Pad(state.ToString(), STATE_CELL_LEN, ' '));
                line.Append("|");
                foreach (var action in actions)
                {
                    line.Append(QTablePrinter.Pad(System.Convert.ToString(qTable.GetQValue(state, action)), ACTION_CELL_LEN, ' '));
                    line.Append('|');
                }
                sb.AppendLine(line.ToString());
            }

            return sb.ToString();
        }

        public static void SaveToFile(QTable qTable, string prettyPath, string path)
        {
            
            var content = QTablePrinter.CreatePrintableRepresentantionOfQTable(qTable);
            FileStream fileStream = new FileStream(prettyPath,
                                    FileMode.OpenOrCreate,
                                    FileAccess.ReadWrite,
                                    FileShare.None);
            StreamWriter writer = new StreamWriter(fileStream);
            writer.Write(content);
            writer.Close();
           //TODO - objects need to be serializable
            //ObjectIOManager.WriteToBinaryFile(path, qTable);            
        }

        public static QTable LoadFromFile(string path)
        {
            QTable qTable = ObjectIOManager.ReadFromBinaryFile<QTable>(path);
            return qTable;
        }
    }
}