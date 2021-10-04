using Root.Reports;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace MeuCantinhoDeEstudos3.Models.PDF
{
    public class Report<TClasse> : Report
        where TClasse : class
    {
        private readonly IList<TClasse> _elementos;

        private FontDef fontDef_Helvetica;
        private const Double rPosLeft = 20;  // millimeters
        private const Double rPosRight = 195;  // millimeters
        private const Double rPosTop = 24;  // millimeters
        private const Double rPosBottom = 278;  // millimeters

        public Report(IList<TClasse> elementos)
            : base()
        {
            _elementos = elementos;
        }

        public static string Create(IList<TClasse> elementos)
        {
            Report<TClasse> report = new Report<TClasse>(elementos);
            HttpContext current = HttpContext.Current;
            string diretorio = current.Server.MapPath("~/Arquivos/Reports/" + typeof(TClasse).Name);
            if (!Directory.Exists(diretorio))
                Directory.CreateDirectory(diretorio);

            string arquivo = Path.Combine(diretorio, $"{Guid.NewGuid()}.pdf");
            report.Save(arquivo);

            return arquivo;
        }

        protected override void Create()
        {
            fontDef_Helvetica = new FontDef(this, FontDef.StandardFont.Helvetica);
            FontProp fontProp_Text = new FontPropMM(fontDef_Helvetica, 1.9);  // standard font
            FontProp fontProp_Header = new FontPropMM(fontDef_Helvetica, 1.9);  // font of the table header
            fontProp_Header.bBold = true;

            // create table
            TableLayoutManager tlm;
            using (tlm = new TableLayoutManager(fontProp_Header))
            {
                tlm.rContainerHeightMM = rPosBottom - rPosTop;  // set height of table
                tlm.tlmCellDef_Header.rAlignV = RepObj.rAlignCenter;  // set vertical alignment of all header cells
                tlm.tlmCellDef_Default.penProp_LineBottom = new PenProp(this, 0.05, Color.LightGray);  // set bottom line for all cells
                tlm.tlmHeightMode = TlmHeightMode.AdjustLast;
                tlm.eNewContainer += new TableLayoutManager.NewContainerEventHandler(Tlm_NewContainer);

                MontarTabela(tlm, fontProp_Text);
            }

            // print page number and current date/time
            Double rY = rPosBottom + 1.5;
            foreach (Page page in enum_Page)
            {
                page.AddLT_MM(rPosLeft, rY, new RepString(fontProp_Text, DateTime.Now.ToShortDateString() + "  " + DateTime.Now.ToShortTimeString()));
                page.AddRT_MM(rPosRight, rY, new RepString(fontProp_Text, page.iPageNo + " / " + iPageCount));
            }
        }

        public virtual void MontarTabela(TableLayoutManager tlm, FontProp fontProp_Text)
        {
            var propriedades = typeof(TClasse).GetProperties().Where(a => a.GetCustomAttributes(typeof(ColunaRelatorioAttribute), true).Count() > 0)
                    .OrderBy(a => ((ColunaRelatorioAttribute)a.GetCustomAttributes(typeof(ColunaRelatorioAttribute), true)[0]).Posicao)
                    .ToList();

            foreach (var propriedade in propriedades)
            {
                var colunaRelatorio = propriedade.GetCustomAttribute<ColunaRelatorioAttribute>(true);
                //tlm.NewLineMM(colunaRelatorio.Titulo, colunaRelatorio.Largura, colunaRelatorio.TextMode);
            }

            foreach (var elemento in _elementos)
            {
                tlm.NewRow();
                foreach (var propriedade in propriedades)
                {
                    var colunaRelatoicio = propriedade.GetCustomAttribute<ColunaRelatorioAttribute>(true);
                    if (propriedade.PropertyType == typeof(string) || propriedade.PropertyType == typeof(Guid))
                        tlm.Add(colunaRelatoicio.Posicao, new RepString(fontProp_Text, propriedade.GetValue(elemento).ToString()));
                    else if (propriedade.PropertyType == typeof(DateTime))
                        tlm.Add(colunaRelatoicio.Posicao, new RepDateTime(fontProp_Text, DateTime.Parse(propriedade.GetValue(elemento).ToString())));
                }
            }
        }

        public void Tlm_NewContainer(Object oSender, TableLayoutManager.NewContainerEventArgs ea)
        {  // only "public" for NDoc, should be "private"
            new Page(this);

            // first page with caption
            if (page_Cur.iPageNo == 1)
            {
                FontProp fontProp_Title = new FontPropMM(fontDef_Helvetica, 5);
                fontProp_Title.bBold = true;
                page_Cur.AddCT_MM(rPosLeft + (rPosRight - rPosLeft) / 2, rPosTop, new RepString(fontProp_Title, "Listagem de Matérias"));
                ea.container.rHeightMM -= fontProp_Title.rLineFeedMM;  // reduce height of table container for the first page
            }

            // the new container must be added to the current page
            page_Cur.AddMM(rPosLeft, rPosBottom - ea.container.rHeightMM, ea.container);
        }
    }
}