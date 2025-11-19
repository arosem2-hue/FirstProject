using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient; // SqlClient 최신 버전 사용

namespace FluentButton2
{
    public partial class FrmMain2 : Form
    {
        private Panel? pnlMenu;

        private readonly string connectionString = "Data Source=이동주\\SQLEXPRESS;Initial Catalog=FirstAppDB;User ID=FirstAppDB;Password=FirstAppDB;TrustServerCertificate=True;";

        public FrmMain2()
        {
            InitializeComponent();
            InitializeLayout();
        }


        private void InitializeLayout()
        {
            this.Text = "FluentButton 메인화면";
            this.Size = new Size(1000, 700);

            pnlMenu = new Panel
            {
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = Color.FromArgb(180, 200, 240),
                AutoScroll = true
            };
            this.Controls.Add(pnlMenu);
        }


        private void FrmMain2_Load(object sender, EventArgs e)
        {
            LoadMenu();
        }

        private void LoadMenu()
        {
            pnlMenu.Controls.Clear();

            List<MenuModel> menuList = GetMenuList();

            var rootMenus = menuList
                .Where(m => m.ParentID == null)
                .OrderBy(m => m.MenuOrder)
                .ToList();

            foreach (var menu in rootMenus)
            {
                // 메인 버튼
                Button btnMain = new Button
                {
                    Text = $"  {menu.MenuName}",
                    Tag = menu.MenuID,
                    Dock = DockStyle.Top,
                    Height = 45,
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleLeft,
                    BackColor = Color.FromArgb(80, 130, 200),
                    ForeColor = Color.White,
                    Font = new Font("맑은 고딕", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnMain.FlatAppearance.BorderSize = 0;
                btnMain.Click += BtnMain_Click;

                // 하위 메뉴 패널
                FlowLayoutPanel pnlSub = new FlowLayoutPanel
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    FlowDirection = FlowDirection.TopDown,
                    Tag = menu.MenuID,
                    Height = 0,
                    MaximumSize = new Size(pnlMenu.Width, 0),
                    BackColor = Color.FromArgb(230, 240, 255),
                    Visible = false
                };

                var subMenus = menuList
                    .Where(s => s.ParentID == menu.MenuID)
                    .OrderBy(s => s.MenuOrder)
                    .ToList();

                foreach (var sub in subMenus)
                {
                    Button btnSub = new Button
                    {
                        Text = $"     ▸ {sub.MenuName}",
                        Tag = sub.FormName,
                        Dock = DockStyle.Top,
                        Height = 35,
                        FlatStyle = FlatStyle.Flat,
                        TextAlign = ContentAlignment.MiddleLeft,
                        BackColor = Color.FromArgb(210, 225, 255),
                        ForeColor = Color.Black,
                        Font = new Font("맑은 고딕", 9)
                    };
                    btnSub.FlatAppearance.BorderSize = 0;
                    btnSub.Click += BtnSub_Click;
                    pnlSub.Controls.Add(btnSub);
                }

                // 순서 중요 (하위 먼저 추가)
                pnlMenu.Controls.Add(pnlSub);
                pnlMenu.Controls.Add(btnMain);
            }
        }

        private void BtnMain_Click(object? sender, EventArgs e)
        {
            if (sender is not Button btnMain) return;

            var menuId = (int)btnMain.Tag;

            // 클릭된 메뉴의 하위 패널 찾기
            var targetPanel = pnlMenu.Controls
                .OfType<FlowLayoutPanel>()
                .FirstOrDefault(p => (int)p.Tag == menuId);

            if (targetPanel == null) return;

            if (!targetPanel.Visible)
            {
                targetPanel.Visible = true;
                AnimatePanelHeight(targetPanel, expand: true);
            }
            else
            {
                AnimatePanelHeight(targetPanel, expand: false);
            }
        }

        private void BtnSub_Click(object? sender, EventArgs e)
        {
            if (sender is not Button btnSub) return;
            string? formName = btnSub.Tag?.ToString();

            MessageBox.Show($"'{formName}' 화면을 여는 로직을 여기에 구현하세요.", "Info");
        }

        /// <summary>
        /// 패널 확장/축소 애니메이션
        /// </summary>
        private async void AnimatePanelHeight(FlowLayoutPanel panel, bool expand)
        {
            int step = 10;
            int targetHeight = expand ? panel.Controls.Count * 35 : 0;

            if (expand)
            {
                for (int h = 0; h <= targetHeight; h += step)
                {
                    panel.Height = h;
                    await Task.Delay(5);
                }
            }
            else
            {
                for (int h = panel.Height; h >= 0; h -= step)
                {
                    panel.Height = h;
                    await Task.Delay(5);
                }
                panel.Visible = false;
            }
        }

        private List<MenuModel> GetMenuList()
        {
            List<MenuModel> list = new();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT MenuID, ParentID, MenuName, FormName, MenuOrder, UseYN FROM MenuMaster WHERE UseYN = 'Y' ORDER BY MenuOrder";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new MenuModel
                    {
                        MenuID = reader.GetInt32(0),
                        ParentID = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                        MenuName = reader.GetString(2),
                        FormName = reader.IsDBNull(3) ? null : reader.GetString(3),
                        MenuOrder = reader.GetInt32(4),
                        UseYN = reader.GetString(5)
                    });
                }
            }
            return list;
        }
    }

    public class MenuModel
    {
        public int MenuID { get; set; }
        public int? ParentID { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public string? FormName { get; set; }
        public int MenuOrder { get; set; }
        public string UseYN { get; set; } = "Y";
    }
}
