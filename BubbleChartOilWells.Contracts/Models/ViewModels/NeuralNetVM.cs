using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleChartOilWells.Contracts.Models.ViewModels
{
    public class NeuralNetVM
    {
        /// <summary>
        /// File for training neuralnet
        /// </summary>
        public string TrainingDataFileName { get; set; }

        /// <summary>
        /// File for prediction
        /// </summary>
        public string PredictDataFileName { get; set; }

        /// <summary>
        /// A vector of integers specifying the number of 
        /// hidden neurons (vertices) in each layer.
        /// </summary>
        public int[] Hidden { get; set; }

        /// <summary>
        /// A numeric value specifying the threshold for 
        /// the partial derivatives of the error function as stopping criteria.
        /// </summary>
        public double Threshold { get; set; }

        /// <summary>
        /// The maximum steps for the training of the neural network.
        /// Reaching this maximum leads to a stop of the neural network's training process.
        /// </summary>
        public int Stepmax { get; set; }

        /// <summary>
        /// The number of repetitions for the neural network's training.
        /// </summary>
        public int Rep { get; set; }

        /// <summary>
        /// A vector containing starting values for the weights.
        /// Set to NULL for random initialization.
        /// </summary>
        public double[] StartWeights { get; set; }

        /// <summary>
        /// A vector or a list containing the lowest and highest limit for the learning rate. 
        /// Used only for RPROP and GRPROP.
        /// </summary>
        public double[] LearningRateLimit { get; set; }

        /// <summary>
        /// A vector or a list containing the multiplication factors for the upper and lower learning rate. 
        /// Used only for RPROP and GRPROP.
        /// </summary>
        public double[] LearningRateFactor { get; set; }

        /// <summary>
        /// A numeric value specifying the learning rate used by traditional backpropagation. 
        /// Used only for traditional backpropagation.
        /// </summary>
        public double LearningRate { get; set; }

        /// <summary>
        /// A string containing the algorithm type to calculate the neural network. 
        /// The following types are possible: 'backprop', 'rprop+', 'rprop-', 'sag', or 'slr'. 
        /// 'backprop' refers to backpropagation, 
        /// 'rprop+' and 'rprop-' refer to the resilient backpropagation with and without weight backtracking,
        /// while 'sag' and 'slr' induce the usage of the modified globally convergent algorithm (grprop).
        /// </summary>
        public string Algorithm { get; set; }

        /// <summary>
        /// A differentiable function that is used for the calculation of the error.
        /// Alternatively, the strings 'sse' and 'ce' which stand for the sum of squared errors and the cross-entropy can be used.
        /// </summary>
        public string ErrorFunction { get; set; }

        /// <summary>
        /// A differentiable function that is used for smoothing the result of the cross product of the covariate or neurons and the weights. 
        /// Additionally the strings, 'logistic' and 'tanh' are possible for the logistic function and tangent hyperbolicus.
        /// </summary>
        public string ActivationFunction { get; set; }

        /// <summary>
        /// Seed
        /// </summary>
        public int Seed { get; set; }
        public double ReachedThreshold { get; set; }
        public int Steps { get; set; }

        #region Errors
        public double Error { get; set; }
        public double TrainMAPE { get; set; }
        public double TrainMAE { get; set; }
        public double TrainSSE { get; set; }
        public double TestMAPE { get; set; }
        public double TestMAE { get; set; }
        #endregion

        /// <summary>
        /// Index of best neural network
        /// </summary>
        public double BestNNIndex { get; set; }

        public List<string> ImportedDataHeaders { get; set; }
        public List<MapVM> selectedMaps { get; set; }
        public DataTable TrainFullResults { get; set; }
        public DataTable TestFullResults { get; set; }
        public DataTable PredictionFullResults { get; set; }
        public List<double> TrainPredicted { get; set; }
        public List<double> TrainActual { get; set; }
        public List<double> TestPredicted { get; set; }
        public List<double> TestActual { get; set; }

        public List<double> TestResults { get; set; }

        #region R objects
        public object NN { get; set; }
        public object InitialTrainData { get; set; }
        #endregion

        public NeuralNetVM()
        {
            Reset();
        }

        public void Reset()
        {
            Hidden = new int[] { 16, 16 };
            Threshold = 0.01;
            Stepmax = 10000000;
            Rep = 25;
            LearningRate = 0.001;
            Algorithm = "backprop";
            ErrorFunction = "sse";
            Error = 0;
            ActivationFunction = "logistic";
            Seed = 12321;
        }
    }
}
