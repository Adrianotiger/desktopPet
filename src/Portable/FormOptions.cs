using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace DesktopPet
{
        /// <summary>
        /// Application options. Need a redesign, so it is not documented.
        /// </summary>
        /// <preliminary/>
    public partial class FormOptions : Form
    {
            /// <summary>
            /// Constructor
            /// </summary>
        public FormOptions()
        {
            InitializeComponent();
        }

            /// <summary>
            /// Restore default animation. Will restore the animation delivered with the app.
            /// </summary>
            /// <param name="sender">Caller object.</param>
            /// <param name="e">Click event values.</param>
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Retry;
            Close();
        }
        
            /// <summary>
            /// New page was loaded. Check if page starts with the -XML- key. If so, the page will be converted to an xml.
            /// </summary>
            /// <param name="sender">Caller as object.</param>
            /// <param name="e">Webpage event values.</param>
        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            WebBrowser web = (WebBrowser)sender;
            string s = web.DocumentText;
            if(s.Substring(0, 5) == "-XML-")
            {
                Program.MyData.SetXml(s.Substring(5), "");
                Program.Mainthread.LoadNewXMLFromString(s.Substring(5));
                Close();
            }
        }

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Brush _textBrush;
            
            // Get the item from the collection.
            TabPage _tabPage = tabControl1.TabPages[e.Index];

            // Use our own font.
            Font _tabFont;


            if (e.State == DrawItemState.Selected)
            {
                // Draw a different background colour, and don't paint a focus rectangle.
                _textBrush = new SolidBrush(Color.Black);
                g.FillRectangle(Brushes.White, e.Bounds);
                _tabFont = new Font(tabControl1.TabPages[e.Index].Font.FontFamily.ToString(), (float)11.0, FontStyle.Bold, GraphicsUnit.Pixel);
            }
            else
            {
                _textBrush = new SolidBrush(Color.Black);
                g.FillRectangle(Brushes.LightGray, e.Bounds);
                _tabFont = new Font(tabControl1.TabPages[e.Index].Font.FontFamily.ToString(), (float)10.0, FontStyle.Regular, GraphicsUnit.Pixel);
            }
            
            // Draw string. Center the text.
            StringFormat _stringFlags = new StringFormat();
            _stringFlags.Alignment = StringAlignment.Center;
            _stringFlags.LineAlignment = StringAlignment.Center;
            g.DrawString(_tabPage.Text, _tabFont, _textBrush, tabControl1.GetTabRect(e.Index), _stringFlags);
        }

        private void FormOptions_Load(object sender, EventArgs e)
        {
                // Set up audio values
            checkBox1.Checked = (Program.MyData.GetVolume() > 0.0);
			trackBar1.Value = (int)(Program.MyData.GetVolume() * 10);
            trackBar1.Enabled = checkBox1.Checked;
			label2.Text = Program.Mainthread.ErrorMessages.AudioErrorMessage;
            if (label2.Text.Length > 1)
            {
                trackBar1.Enabled = false;
                checkBox1.Enabled = false;
            }
			checkBox2.Checked = Program.MyData.GetWindowForeground();
            checkBox4.Checked = Program.MyData.GetStealTaskbarFocus();
            trackBar2.Value = Program.MyData.GetAutoStartPets();
            trackBar3.Tag = Program.MyData.GetScale();
            trackBar3.Value = Program.MyData.GetScale();
            label5.Text = trackBar2.Value.ToString();
            label2.Text = trackBar1.Value.ToString();
            label9.Text = Math.Pow(2, (trackBar3.Value - 1)).ToString() + "x";
            checkBox3.Checked = Program.MyData.GetMultiscreen();
            /*
            webBrowser1.DocumentText = "<script>" +
                "alert(1);fetch('https://raw.githubusercontent.com/Adrianotiger/desktopPet/master/Pets/pets.json').then(f=>{" +
                "alert(f);" +
                "});" +
                "</script>";
            */
            LoadWebViewPage();
        }

        private async void LoadWebViewPage()
        {
            await webView21.EnsureCoreWebView2Async();

            var script = "let pets = []; const url='https://raw.githubusercontent.com/Adrianotiger/desktopPet/master/Pets/';\n" +
                "function loadPetImage(url,im){var img = new Image();img.addEventListener('load', ()=>{im.src = img.src;}); img.src=url;}\n" +
                "function loadPetInfo(path) { var xobj = new XMLHttpRequest(); xobj.onreadystatechange = () => { " +
                    "if (xobj.readyState === 4 && xobj.status === 200) {" +
                    "var parser = new DOMParser(); xmlText=xobj.responseText; var xmlDoc = parser.parseFromString(xmlText, 'text/xml'); console.log(xmlDoc);" +
                    "var tr, td; let h = xmlDoc.getElementsByTagName('header')[0]; const x = document.getElementById('xmldiv'); x.innerHTML=''; " +
                    "let i = document.createElement('img'); i.src='data:image/icon;base64,'+h.getElementsByTagName('icon')[0].textContent; x.appendChild(i); x.appendChild(document.createElement('br'));" +
                    "let t = document.createElement('table'); tr=document.createElement('tr'); td=document.createElement('td'); td.appendChild(document.createTextNode('Author :')); tr.appendChild(td); td=document.createElement('td'); td.appendChild(document.createTextNode(h.getElementsByTagName('author')[0].textContent)); tr.appendChild(td); t.appendChild(tr);" +
                    "tr=document.createElement('tr'); td=document.createElement('td'); td.appendChild(document.createTextNode('Project:')); tr.appendChild(td); td=document.createElement('td'); td.appendChild(document.createTextNode(h.getElementsByTagName('title')[0].textContent)); tr.appendChild(td); t.appendChild(tr);" +
                    "tr=document.createElement('tr'); td=document.createElement('td'); td.appendChild(document.createTextNode('Pet name:')); tr.appendChild(td); td=document.createElement('td'); td.appendChild(document.createTextNode(h.getElementsByTagName('petname')[0].textContent)); tr.appendChild(td); t.appendChild(tr);" +
                    "tr=document.createElement('tr'); td=document.createElement('td'); td.appendChild(document.createTextNode('Version:')); tr.appendChild(td); td=document.createElement('td'); td.appendChild(document.createTextNode(h.getElementsByTagName('version')[0].textContent)); tr.appendChild(td); t.appendChild(tr);" +
                    "tr=document.createElement('tr'); td=document.createElement('td'); td.appendChild(document.createTextNode('Size:')); tr.appendChild(td); td=document.createElement('td'); td.appendChild(document.createTextNode(parseInt(xobj.responseText.length / 1024) + 'kb')); tr.appendChild(td); t.appendChild(tr);" +
                    "x.appendChild(t); x.appendChild(document.createElement('br')); x.appendChild(document.createElement('br'));" +
                    "const regex = /\\[(br|link:).*?(?=])]/gm; var info = h.getElementsByTagName('info')[0].textContent; var info2=''; let m; var ind = 0;" +
                    "while ((m = regex.exec(info)) !== null) { if (m.index === regex.lastIndex) regex.lastIndex++; " + 
                      "console.log(`Found match - ${m}`, m); " + 
                      "if(m[1] == 'br') {x.appendChild(document.createTextNode(info.substring(ind, m.index))); x.appendChild(document.createElement('br')); ind = m.index+4; }" +
                      "if(m[1] == 'link:') {var a2=document.createElement('a'); var a2s=m[0].substring(6,m[0].length-1); a2.setAttribute('href', a2s); a2.setAttribute('target', '_blank'); a2.appendChild(document.createTextNode(a2s)); x.appendChild(document.createTextNode(info.substring(ind, m.index))); x.appendChild(a2); ind = m.index+4; }" +
                    "}" + 
                    "x.appendChild(document.createElement('hr'));" +
                    "var a=document.createElement('a'); a.setAttribute('href', '-XML-'+xmlText); a.setAttribute('style', 'display:inline-block;height:40px;width:65vw;border-radius:20px;background:linear-gradient(to bottom, #aaff00, #004000);color:white;border-style:solid;border-color:black;border-width:2px;padding-top:10px;');" +
                    "var isrc='data:image/png;base64,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';" +
                    "var i2=document.createElement('img'); i2.setAttribute('src', isrc); i2.setAttribute('style', 'vertical-align:middle;'); " +
                    "a.appendChild(i2); a.appendChild(document.createTextNode(' DOWNLOAD '));" +
                    "var i2=document.createElement('img'); i2.setAttribute('src', isrc); i2.setAttribute('style', 'vertical-align:middle;'); " +
                    "a.appendChild(i2); x.appendChild(a); " +
                "x.style.display ='block'; " +
                "} }; xobj.open('GET', path, true); xobj.send(null);}" +
                "fetch(url+'pets.json').then(f=>f.json()).then(j=>{" +
                "console.log(j);j.pets.forEach(p=>pets.push(p));" +
                "pets.sort((a,b)=>a.lastupdate<b.lastupdate?1:(a.lastupdate>b.lastupdate?-1:0));" +
                "pets.forEach(p=>{" +
                "var tr; var td; let d=document.createElement('div'); let i=document.createElement('img'); let t= document.createElement('table');" +
                "d.className='aniicon'; d.id=p.folder; d.addEventListener('click',()=>{loadPetInfo(url+p.folder+'/animations.xml')}); " +
                "i.src='data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+ip1sAAAAASUVORK5CYII='; i.id=p.folder+'ico';" +
                "tr=document.createElement('tr'); td=document.createElement('td'); td.appendChild(document.createTextNode('')); tr.appendChild(td); t.appendChild(tr);" +
                "tr=document.createElement('tr'); td=document.createElement('td'); td.appendChild(document.createTextNode(p.author)); tr.appendChild(td); t.appendChild(tr);" +
                "tr=document.createElement('tr'); td=document.createElement('td'); td.appendChild(document.createTextNode(p.lastupdate)); tr.appendChild(td); t.appendChild(tr);" +
                "d.appendChild(i); d.appendChild(t); document.body.appendChild(d);" + 
                "setTimeout(()=>{loadPetImage(url+p.folder+'/icon.png', i)}, 20);" +
                "});" +
                "});";
            var style = "body {width:98vw;margin:0 auto;text-align:center; overflow-x:hidden;}" +
                        ".aniicon {width: 70px; height: 90px; display: inline-block; background: linear-gradient(to bottom, #ccccff, #ffffcc);background-color:#ddddff;border-radius:5px;cursor:pointer;text-align:center;margin:3px;border-radius:8px;transition:0.5s ease-in-out;box-shadow:5px 5px 5px grey;}" +
                        ".aniicon:hover {background: linear-gradient(to bottom, #ffffee, #ffff88);background-color:#ffffcc;box-shadow:0px 0px 5px grey;transform:translate(3px,3px);}" +
                        ".aniicon img { max-height:48px; max-width:48px; vertical-align:middle; }" +
                        ".aniicon td { display: block; overflow: hidden; white-space:nowrap; width: 68px; height: 12px; text-overflow:ellipsis; text-align:center; font-size:10px; padding: 0px; margin: 0px; }" +
                        ".xmldiv {display: none; position: fixed; width: 80vw; height: 80vh; top: 10vh; left: 10vw; overflow: auto; background-color:#ddddff;text-align:center;margin:0 auto;border-style:ridge;border-width:3px;border-radius:20px;}" +
                        ".xmldiv table { margin: 0 auto; left:0px; right: 0px; border-style:ridge; border-width:2px; border-radius:4px; font-weight:bold; }";

                        webView21.NavigateToString("<style>"+style+"</style><script>"+script+ "</script><div class='xmldiv' id='xmldiv' onclick='this.style.display=none'></div>");
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            trackBar1.Enabled = checkBox1.Checked;
            if(!trackBar1.Enabled)
            {
                trackBar1.Value = 0;
                trackBar1_Scroll(sender, e);
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            Program.MyData.SetVolume((float)(trackBar1.Value / 10.0));
            if(Program.MyData.GetVolume() < 0.1f)
            {
                trackBar1.Enabled = false;
                checkBox1.Checked = false;
            }
            label2.Text = trackBar1.Value.ToString();
        }

		private void checkBox2_Click(object sender, EventArgs e)
		{
            Program.MyData.SetWindowForeground(checkBox2.Checked);
		}

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            Program.MyData.SetStealTaskbarFocus(checkBox4.Checked);
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
		{
            Program.MyData.SetAutoStartPets(trackBar2.Value);
            label5.Text = trackBar2.Value.ToString();
		}

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            Program.MyData.SetMultiscreen(checkBox3.Checked);
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            Program.MyData.SetScale(trackBar3.Value);
            label9.Text = Math.Pow(2, (trackBar3.Value - 1)).ToString() + "x";

            MessageBox.Show("Scale changed. Application will be restarted", "New scale", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            using (var petProcess = new Process())
            {
                petProcess.StartInfo.FileName = Application.ExecutablePath;
                petProcess.Start();
            }

            Hide();
            Application.Exit();
        }
    }
}
