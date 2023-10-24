using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.IAJ.Unity.DecisionMaking.RL;
using System.IO;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Utils
{
    public static class QTablePrinter
    {
        static int STATE_CELL_LEN = 100;
        static int ACTION_CELL_LEN = 20;
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
                line.Append(state.ToString());
                line.Append(QTablePrinter.Pad(line.ToString(), STATE_CELL_LEN, ' '));
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

        public static void SaveToFile(QTable qTable, string path, bool printableRepresentation)
        {
            if (printableRepresentation)
            {
                var content = QTablePrinter.CreatePrintableRepresentantionOfQTable(qTable);
                File.WriteAllText(path, content);
            }
            else
            {
                ObjectIOManager.WriteToBinaryFile(path, qTable);
            }
        }

        public static QTable LoadFromFile(string path)
        {
            QTable qTable = ObjectIOManager.ReadFromBinaryFile<QTable>(path);
            return qTable;
        }
    }
}