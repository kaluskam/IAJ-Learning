using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.IAJ.Unity.DecisionMaking.RL;
using System.Collections.Generic;
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
        public static List<Action> ACTIONS;
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
                sb.Append(Pad(action.Name, ACTION_CELL_LEN, ' ') + '|');
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
            
            var content = CreatePrintableRepresentantionOfQTable(qTable);
            FileStream fileStream = new FileStream(prettyPath,
                                    FileMode.OpenOrCreate,
                                    FileAccess.ReadWrite,
                                    FileShare.None);
            StreamWriter writer = new StreamWriter(fileStream);
            writer.Write(content);
            writer.Close();

            Dictionary<(RLState, string), float> qTableToSave = new Dictionary<(RLState, string), float>();
            foreach (var key in qTable.values.Keys)
            {
                qTableToSave.Add((key.Item1, key.Item2.Name), qTable.values[key]);
            }
           //TODO - objects need to be serializable
           ObjectIOManager.WriteToBinaryFile(path, qTableToSave);            
        }

        public static QTable LoadFromFile(string path)
        {
            QTable qTable = new QTable();
            Dictionary<(RLState, string), float> qTableValues = ObjectIOManager.ReadFromBinaryFile<Dictionary<(RLState, string), float>>(path);
            foreach (var key in qTableValues.Keys)
            {
                Action action = null;
                foreach(var a in ACTIONS)
                {
                    if (a.Name == key.Item2)
                    {
                        action = a;
                        break;
                    }
                }
                qTable.SetQValue(key.Item1, action, qTableValues[key]);
            }
            return qTable;
        }
    }
}