using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D; // Add this namespace for Region  



namespace RestaurantApp
{
    public static class Utils
    {

        public static string CreateMD5(string input) // şifreleri encode etmek (tek yönlü) için kullanılır
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            //input değerinin hash hesabını yapar
            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(input));


            //hesaplandıktan sonra byte array'e aktarır
            byte[] result = md5.Hash;

            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                //her byte değer X2 onaltılı sayı olarak dönüştürür

                strBuilder.Append(result[i].ToString("x2"));
            }

            return strBuilder.ToString();
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            string toDecode = base64EncodedData.Substring(2, base64EncodedData.Length - 2);
            var base64EncodedBytes = System.Convert.FromBase64String(toDecode);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static void RoundTextBox(TextBox txtBox, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(txtBox.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(txtBox.Width - radius, txtBox.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, txtBox.Height - radius, radius, radius, 90, 90);
            path.CloseAllFigures();
            txtBox.Region = new Region(path);
        }



        public static void ApplyDefaultStyle(DataGridView dgv)
        {
            dgv.EnableHeadersVisualStyles = false;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = Color.FromArgb(230, 230, 230);
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.LightGray;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(20, 0, 0, 0);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersHeight = 60;
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = dgv.ColumnHeadersDefaultCellStyle.BackColor;

            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = Color.Black;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(245, 245, 245);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            dgv.DefaultCellStyle.Padding = new Padding(20, 0, 0, 0);

            dgv.RowTemplate.Height = 40;
            dgv.RowHeadersVisible = false;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.Dock = DockStyle.Fill;
            dgv.DefaultCellStyle.SelectionBackColor = Color.LightGray;
        }


        public static void SetSingleRowSelection(DataGridView dgv)
        {
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
        }



        public static class DataGridViewButtonImageHelper
        {
            /// <summary>
            /// DataGridView'de buton hücresine isme göre image çizer.
            /// </summary>
            /// <param name="e">CellPaintingEventArgs</param>
            /// <param name="buttonColumnName">Butonun column adı</param>
            /// <param name="image">Çizilecek image</param>
            public static void DrawButtonImage(DataGridViewCellPaintingEventArgs e, string buttonColumnName, Image image, DataGridView grid)
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0)
                    return;

                if (grid.Columns[e.ColumnIndex].Name == buttonColumnName)
                {
                    e.PaintBackground(e.ClipBounds, true);
                    int w = image.Width;
                    int h = image.Height;
                    int x = e.CellBounds.Left + (e.CellBounds.Width - w) / 2;
                    int y = e.CellBounds.Top + (e.CellBounds.Height - h) / 2;
                    e.Graphics.DrawImage(image, new Rectangle(x, y, w, h));
                    e.Handled = true;
                }
            }
        }




    }




}

