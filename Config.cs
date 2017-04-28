using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MicroConfig
{
    /// <summary>
    /// Třída použitá jako konfig musí být podtřídou třídy Config
    /// Aby bylo pole uloženo, musí být veřejné a jméno nezačínat znakem "_"
    /// Všechna ukládaná pole musí mít počáteční hodnotu
    /// Uložení: Config.Save(instanceofyourconfig);
    /// Načtení: instanceofyourconfig.Deserialize();
    /// Jméno souboru je shodné se jménem třídy
    ///     např. class Main -> Main.cfg
    /// Všechny konfigy uložené metodou Save jsou ve složce configs v CWD
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Serializuje objekt
        /// </summary>
        /// <param name="what">
        ///     objekt jehož pole mají být serializována
        ///     je vyžadováno aby typ objektu byl podtřídou třídy Config
        /// </param>
        /// <returns>Textový řetězec serializovaných hodnot</returns>
        public static string Serialize(object what)
        {
            StringBuilder sb = new StringBuilder();
            if (what.GetType().BaseType == typeof(Config))
                foreach (FieldInfo fld in what.GetType().GetFields())
                    if (fld.IsPublic && !fld.Name.StartsWith("_") && fld.IsStatic == false)
                    {
                        if (fld.GetValue(what).GetType() != typeof(string))
                            sb.AppendLine(string.Format("{0} = {1}", fld.Name, fld.GetValue(what).ToString()));
                        else
                            sb.AppendLine(string.Format("{0} = {1}", fld.Name, "\"" + fld.GetValue(what).ToString() + "\""));
                    }
            else
                throw new Exception("A serialized object must be an instance of a class extending Config");
            return sb.ToString();
        }
        /// <summary>
        /// Metoda pro uložení konfigu.
        /// </summary>
        /// <param name="what">instance třídy rozšiřující Config</param>
        public static void Save(object what)
        {
            StreamWriter str = File.CreateText(Path.Combine(Environment.CurrentDirectory, "configs", what.GetType().Name + ".cfg"));
            str.WriteLine(Serialize(what));
            str.Flush();
            str.Close();
        }

        /// <summary>
        /// Metoda pro serializaci do jiné složky
        /// </summary>
        /// <param name="what">serializovaný object - instance třídy rozšiřující třídu Config</param>
        /// <param name="path">relativní cesta ke složce</param>
        public static void Save(object what, string path)
        {
            StreamWriter str = File.CreateText(Path.Combine(Environment.CurrentDirectory, path, what.GetType().Name + ".cfg"));
            str.WriteLine(Serialize(what));
            str.Flush();
            str.Close();
        }

        public void Deserialize()
        {
            string[] strs = File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, "configs", this.GetType().Name + ".cfg"));
            Dictionary<string, object> fldlist = new Dictionary<string, object>();
            foreach (string str in strs)
            {
                string name = str.Replace(" = ", "=").Split('=')[0];
                string strvalue = string.Join("=", str.Replace(" = ", "=").Split('=').Skip(1).ToArray());
                object value = new object();
                switch (strvalue)
                {
                    case "True":
                        value = true;
                        break;
                    case "False":
                        value = false;
                        break;
                    default:
                        int num;
                        if (!int.TryParse(strvalue, out num))
                            if (strvalue.EndsWith("\"") && strvalue.StartsWith("\""))
                                value = strvalue.Substring(1, strvalue.Length - 2);
                        else
                            value = num;
                        break;
                }
                fldlist.Add(name, value);
            }
            try
            {
                foreach (KeyValuePair<string, object> pair in fldlist)
                    GetType().GetField(pair.Key).SetValue(this, pair.Value);
            }
            catch { }
        }

        public void Deserialize(string dirpath)
        {
            string[] strs = File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, dirpath, this.GetType().Name + ".cfg"));
            Dictionary<string, object> fldlist = new Dictionary<string, object>();
            foreach (string str in strs)
            {
                string name = str.Replace(" = ", "=").Split('=')[0];
                string strvalue = string.Join("=", str.Replace(" = ", "=").Split('=').Skip(1).ToArray());
                object value = new object();
                switch (strvalue)
                {
                    case "True":
                        value = true;
                        break;
                    case "False":
                        value = false;
                        break;
                    default:
                        int num;
                        if (!int.TryParse(strvalue, out num))
                            if (strvalue.EndsWith("\"") && strvalue.StartsWith("\""))
                                value = strvalue.Substring(1, strvalue.Length - 2);
                        else
                            value = num;
                        break;
                }
                fldlist.Add(name, value);
            }
            try
            {
                foreach (KeyValuePair<string, object> pair in fldlist)
                    GetType().GetField(pair.Key).SetValue(this, pair.Value);
            }
            catch { }
        }

        public Config(){}

        public Config(bool autoload)
        {
            if (autoload)
                if (!File.Exists(Path.Combine(Environment.CurrentDirectory, string.Format("configs/{0}.cfg", GetType().Name))))
                {
                    Config.Save(this);
                    this.Deserialize();
                }
                else
                {
                    this.Deserialize();
                    Config.Save(this);
                }
        }
    }
}

