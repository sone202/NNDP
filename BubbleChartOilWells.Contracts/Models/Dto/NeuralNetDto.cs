using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleChartOilWells.Contracts.Models.Dto
{
    public class NeuralNetDto
    {
        public string trainingDataFileName;

        public string initialDataFileName;

        public string resultDataFilename;

        public int[] hiddenLayers;

        public double threshold;

        public string algorythm;

        public string errorFunction;

        public double resultErrorThreshold;
    }
}
