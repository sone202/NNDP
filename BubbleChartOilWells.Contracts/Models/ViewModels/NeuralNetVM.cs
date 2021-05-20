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
        public string trainingDataFileName { get; set; }

        /// <summary>
        /// File for prediction
        /// </summary>
        public string initialDataFileName { get; set; }

        /// <summary>
        /// A vector of integers specifying the number of 
        /// hidden neurons (vertices) in each layer.
        /// </summary>
        public int[] hidden { get; set; }

        /// <summary>
        /// A numeric value specifying the threshold for 
        /// the partial derivatives of the error function as stopping criteria.
        /// </summary>
        public double threshold { get; set; }

        /// <summary>
        /// The maximum steps for the training of the neural network.
        /// Reaching this maximum leads to a stop of the neural network's training process.
        /// </summary>
        public int stepmax { get; set; }

        /// <summary>
        /// The number of repetitions for the neural network's training.
        /// </summary>
        public int rep { get; set; }

        /// <summary>
        /// A vector containing starting values for the weights.
        /// Set to NULL for random initialization.
        /// </summary>
        public double[] startWeights { get; set; }

        /// <summary>
        /// A vector or a list containing the lowest and highest limit for the learning rate. 
        /// Used only for RPROP and GRPROP.
        /// </summary>
        public double[] learningRateLimit { get; set; }

        /// <summary>
        /// A vector or a list containing the multiplication factors for the upper and lower learning rate. 
        /// Used only for RPROP and GRPROP.
        /// </summary>
        public double[] learningRateFactor { get; set; }

        /// <summary>
        /// A numeric value specifying the learning rate used by traditional backpropagation. 
        /// Used only for traditional backpropagation.
        /// </summary>
        public double learningRate { get; set; }

        /// <summary>
        /// A string containing the algorithm type to calculate the neural network. 
        /// The following types are possible: 'backprop', 'rprop+', 'rprop-', 'sag', or 'slr'. 
        /// 'backprop' refers to backpropagation, 
        /// 'rprop+' and 'rprop-' refer to the resilient backpropagation with and without weight backtracking,
        /// while 'sag' and 'slr' induce the usage of the modified globally convergent algorithm (grprop).
        /// </summary>
        public string algorithm { get; set; }

        /// <summary>
        /// A differentiable function that is used for the calculation of the error.
        /// Alternatively, the strings 'sse' and 'ce' which stand for the sum of squared errors and the cross-entropy can be used.
        /// </summary>
        public string errorFunction { get; set; }

        /// <summary>
        /// A differentiable function that is used for smoothing the result of the cross product of the covariate or neurons and the weights. 
        /// Additionally the strings, 'logistic' and 'tanh' are possible for the logistic function and tangent hyperbolicus.
        /// </summary>
        public string activationFunction { get; set; }

        /// <summary>
        /// Result error
        /// </summary>
        public double error { get; set; }

        public double reachedThreshold { get; set; }

        public int steps { get; set; }

        public double MAPE { get; set; }

        public DataTable TrainResult { get; set; }
        public DataTable TestResult { get; set; }
        public DataTable PredictionResult { get; set; }
        public object Model { get; set; }

        public NeuralNetVM()
        {
            Reset();
        }

        public void Reset()
        {
            hidden = new int[] { 64, 64 };
            threshold = 0.01;
            stepmax = 100000;
            rep = 1;
            learningRate = 0.01;
            algorithm = "backprop";
            errorFunction = "sse";
            activationFunction = "logistic";
            error = 0;
        }
    }
}
