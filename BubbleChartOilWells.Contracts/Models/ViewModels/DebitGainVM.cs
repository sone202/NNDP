using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleChartOilWells.Contracts.Models.ViewModels
{
    /// <summary>
    /// Прирост дебита жидкости для ННС для радиального псевдоустановившегося притока
    /// 
    /// q = K*H*(Pr-Pwf)/(18.41*(ln(re/rw)-0.75+Stot))*(Ko/(MUo*Bo)+Kw/(MUw*Bw))
    /// 
    ///            KH(Pr-Pwf)                Ko          Kw                    
    /// q = ___________________________ * (________ + ________)            
    ///     18.41*(ln(re/rw)-0.75+Stot)    MUo* Bo     MUw* Bw
    /// </summary>
    public class DebitGainVM
    {
        public MapVM KH { get; set; }
        public MapVM K { get; set; }
        public MapVM H { get; set; }
        public MapVM Pr { get; set; }
        public double Pwf { get; set; }
        public double Re { get; set; }
        public double Rw { get; set; }
        public double Stot { get; set; }
        public List<double> KoInEachMapCell { get; set; }
        public List<double> KwInEachMapCell { get; set; }
        public double Krwor { get; set; }
        public double Krocw { get; set; }
        public double Sor { get; set; }
        public double Scw { get; set; }
        public MapVM Sw { get; set; }
        public double MUo { get; set; }
        public double MUw { get; set; }
        public double Bo { get; set; }
        public double Bw { get; set; }
        public double Nw { get; set; }
        public double No { get; set; }
        public DataTable Ofp { get; set; }

        public DebitGainVM()
        {
            Reset();
        }

        
        public void Reset()
        {
            Pwf = 110;
            Re = 250;
            Rw = 0.1;
            Stot = 0;
            KoInEachMapCell = new List<double>();
            KwInEachMapCell = new List<double>();
            MUo = 2.2;
            MUw = 1.33;
            Bo = 1.052;
            Bw = 1.01;
            Ofp = CreateEmptyDataTable();
            Nw = 0;
            No = 0;
        }

        private DataTable CreateEmptyDataTable()
        {
            var dt = new DataTable();

            dt.Columns.Add("Водонасыщенность", typeof(double));
            dt.Columns.Add("ОФП нефти", typeof(double));
            dt.Columns.Add("ОФП воды", typeof(double));

            for (int i = 0; i < 8; i++)
            {
                dt.Rows.Add(dt.NewRow());
            }

            // TODO: Delete
            // dt.Rows[0][0] = 0.11;
            // dt.Rows[1][0] = 0.20;
            // dt.Rows[2][0] = 0.25;
            // dt.Rows[3][0] = 0.31;
            // dt.Rows[4][0] = 0.36;
            // dt.Rows[5][0] = 0.42;
            // dt.Rows[6][0] = 0.51;
            //
            // dt.Rows[0][1] = 1.00;
            // dt.Rows[1][1] = 0.46;
            // dt.Rows[2][1] = 0.28;
            // dt.Rows[3][1] = 0.17;
            // dt.Rows[4][1] = 0.12;
            // dt.Rows[5][1] = 0.07;
            // dt.Rows[6][1] = 0.00;
            //
            // dt.Rows[0][2] = 0.00;
            // dt.Rows[1][2] = 0.03;
            // dt.Rows[2][2] = 0.04;
            // dt.Rows[3][2] = 0.06;
            // dt.Rows[4][2] = 0.08;
            // dt.Rows[5][2] = 0.10;
            // dt.Rows[6][2] = 0.24;
            

            dt.Rows[0][0] = 0.22;
            dt.Rows[1][0] = 0.3;
            dt.Rows[2][0] = 0.35;
            dt.Rows[3][0] = 0.4;
            dt.Rows[4][0] = 0.45;
            dt.Rows[5][0] = 0.5;
            dt.Rows[6][0] = 0.55;
            dt.Rows[7][0] = 0.61;

            dt.Rows[0][1] = 1.00;
            dt.Rows[1][1] = 0.605;
            dt.Rows[2][1] = 0.345;
            dt.Rows[3][1] = 0.14;
            dt.Rows[4][1] = 0.09;
            dt.Rows[5][1] = 0.044;
            dt.Rows[6][1] = 0.016;
            dt.Rows[7][1] = 0.005;

            dt.Rows[0][2] = 0.00;
            dt.Rows[1][2] = 0.033;
            dt.Rows[2][2] = 0.053;
            dt.Rows[3][2] = 0.072;
            dt.Rows[4][2] = 0.09;
            dt.Rows[5][2] = 0.112;
            dt.Rows[6][2] = 0.132;
            dt.Rows[7][2] = 0.167;

            return dt;
        }
    }
}
