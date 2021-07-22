using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.Protocols;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DNUserViewer {
	public partial class Form1 : Form {

        Dictionary<string, string> exportDict = new Dictionary<string, string>();
        public Form1() {
			InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            this.textBoxDomain.Text = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
            var name = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            var slashIndex = name.IndexOf("\\");
            this.textBoxUsername.Text = slashIndex > -1 ? name.Substring(slashIndex + 1) : name.Substring(0, name.IndexOf("@"));
            this.textBoxQueryUser.Text = slashIndex > -1 ? name.Substring(slashIndex + 1) : name.Substring(0, name.IndexOf("@"));
        }

        protected void ShowLDAPError(LdapException lexc) {
            if (lexc.ToString().Contains("The LDAP server is unavailable")) {
                MessageBox.Show("The LDAP server is unavailable", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            String error = lexc.ServerErrorMessage;
            string pp = error.Substring(76, 4);
            string ppp = pp.Trim();

            if ("52e" == ppp) MessageBox.Show("Invalid Username or password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            if ("775​" == ppp) MessageBox.Show("User account locked.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            if ("525​" == ppp) MessageBox.Show("User not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            if ("530" == ppp) MessageBox.Show("Not permitted to logon at this time.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            if ("531" == ppp) MessageBox.Show("Not permitted to logon at this workstation.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            if ("532" == ppp) MessageBox.Show("Password expired.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            if ("533​" == ppp) MessageBox.Show("Account disabled.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        public static string ByteArrayToString(byte[] ba) {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        private void Query_User(String username, String password, String domain, String query) {

            try {
                if (username == "") {
                    MessageBox.Show("Missing Username.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                } else if (password == "") {
                    MessageBox.Show("Missing password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                } else {
                    LdapConnection connection = new LdapConnection(domain);
                    NetworkCredential credential = new NetworkCredential(username, password);
                    connection.Credential = credential;
                    connection.Bind();

                    // after authenticate Loading user details to data table
                    PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
                    UserPrincipal ld_user = UserPrincipal.FindByIdentity(ctx, query);
                    DirectoryEntry up_User = (DirectoryEntry)ld_user.GetUnderlyingObject();
                    DirectorySearcher deSearch = new DirectorySearcher(up_User);
                    SearchResultCollection results = deSearch.FindAll();

                    exportDict = new Dictionary<string, string>();
                    buttonExport.Enabled = false;

                    //int i = 0;
                    if (results != null && results.Count > 0) {
                        buttonExport.Enabled = true;
                        DataTable dt = new DataTable();
                        dt.Columns.Add("Key");
                        dt.Columns.Add("Value");

                        foreach (string rp in results[0].Properties.PropertyNames) {
                            DataRow row = dt.NewRow();
                            DataRow raw_row = dt.NewRow();
                            bool add_raw = false;
                            object r = results[0].Properties[rp][0];
                            row["Key"] = rp.ToString();


                            switch (r.GetType().ToString()) {
                                case "System.DateTime":
                                    row["Value"] = ((DateTime)(r)).ToLocalTime().ToString();
                                    //Add gmt value
                                    raw_row["Key"] = rp.ToString() + "_GMT";
                                    raw_row["Value"] = r.ToString();
                                    add_raw = true;
                                    exportDict.Add(rp.ToString() + "_GMT", r.ToString());
                                    break;
                                case "System.Int64":
                                    raw_row["Key"] = rp.ToString()+"_RAW";
                                    raw_row["Value"] = r.ToString();
                                    add_raw = true;
                                    exportDict.Add(rp.ToString() + "_RAW", r.ToString());

                                    if ((Int64)r > 110133216000000000) { // Assume LDAP timestamp is value is bigger than year 1950
                                        DateTime datetime = new DateTime(1601, 01, 01, 0, 0, 0, DateTimeKind.Utc).AddTicks((Int64)r);
                                        row["Value"] = datetime.ToLocalTime().ToString();
                                        exportDict.Add(rp.ToString(), datetime.ToLocalTime().ToString());
                                    } else {
                                        row["Value"] = r.ToString();
                                        exportDict.Add(rp.ToString(), r.ToString());
                                    }
                                    break;
                                case "System.String":
                                    row["Value"] = r.ToString();
                                    exportDict.Add(rp.ToString(), r.ToString());
                                    break;
                                case "System.Byte[]":
                                    row["Value"] = ByteArrayToString((System.Byte[])r);
                                    exportDict.Add(rp.ToString(), ByteArrayToString((System.Byte[])r));
                                    break;
                                default:
                                    Console.WriteLine(rp + " / " + r.GetType());
                                    row["Value"] = r.ToString();
                                    exportDict.Add(rp.ToString(), r.ToString());
                                    break;
                            }
                            dt.Rows.Add(row);
                            if(add_raw) dt.Rows.Add(raw_row);
                        }
                        dataGridView1.DataSource = dt;
                        dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                        dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        dataGridView1.Columns[0].ReadOnly = true;
                        dataGridView1.Columns[1].ReadOnly = true;
                        dataGridView1.ReadOnly = true;
                    }

                }

            } catch (LdapException lexc) {
                ShowLDAPError(lexc);
            }/* catch (Exception) {
                MessageBox.Show("Invalid Username or password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }*/
        }

        private void buttonQueryUsername_Click(object sender, EventArgs e) {
            Query_User(textBoxUsername.Text, textBoxPassword.Text, textBoxDomain.Text, textBoxQueryUser.Text);
        }

		private void buttonExport_Click(object sender, EventArgs e) {
            saveFileDialog1.FileName = "export_" + textBoxQueryUser.Text + ".json";
            saveFileDialog1.ShowDialog();
        }

		private void saveFileDialog1_FileOk(object sender, CancelEventArgs e) {
            File.WriteAllText(saveFileDialog1.FileName, Newtonsoft.Json.JsonConvert.SerializeObject(exportDict, Newtonsoft.Json.Formatting.Indented));
        }
	}
}
