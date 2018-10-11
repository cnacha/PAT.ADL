using PAT.Common.Classes.DataStructure;
using PAT.ADL.LTS;

namespace PAT.ADL.Assertions{
    public class DataStore
    {
        public static DataStore DataManager;

        public StringDictionary<Process> DefinitionInstanceDatabase;
        public StringDictionary<string> ExpressionHashTable;

        public string LastProcessString;

        public DataStore()
        {
            ExpressionHashTable = new StringDictionary<string>(PAT.Common.Classes.Ultility.Ultility.MC_INITIAL_SIZE);
            DefinitionInstanceDatabase = new StringDictionary<Process>();
        }

        public void ClearVisitedTable()
        {
            LastProcessString = "";
        }

        /// <summary>
        /// Hash a given string representing a Process Expression to the length of a singple integer. 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string InitializeProcessID(string key)
        {
            LastProcessString = key;
            string value = ExpressionHashTable.GetContainsKey(key);
            if (value != null)
            {
                return value;
            }
            else
            {
                string id = ExpressionHashTable.Count.ToString();
                ExpressionHashTable.Add(key, id);
                return id;
            }
        }

        public bool SetLastProcessID(string id)
        {
            return ExpressionHashTable.SetValue(LastProcessString, id);
        }
    }

}