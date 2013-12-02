﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DirectOutput.Cab.Out;
using System.Xml.Serialization;

namespace DirectOutput.Cab.Toys.Layer
{
    /// <summary>
    /// Represents a adressable led strip. 
    /// 
    /// The toy supports several layers and supports transparency/alpha channels for every single led.
    /// </summary>
    public class LedStrip : ToyBaseUpdatable, IToy
    {
        #region Config properties
        private int _Width = 1;

        /// <summary>
        /// Gets or sets the width resp. number of leds in horizontal direction of the led stripe. 
        /// </summary>
        /// <value>
        /// The width of the led stripe.
        /// </value>
        public int Width
        {
            get { return _Width; }
            set { _Width = value.Limit(0, int.MaxValue); }
        }

        private int _Height = 1;

        /// <summary>
        /// Gets or sets the height resp. the number of leds in vertical direction of the led strip.
        /// </summary>
        /// <value>
        /// The height of the led stripe.
        /// </value>
        public int Height
        {
            get { return _Height; }
            set { _Height = value.Limit(0, int.MaxValue); }
        }


        /// <summary>
        /// Gets the number of leds of the led stripe.
        /// </summary>
        /// <value>
        /// The number of leds of the led stripe.
        /// </value>
        public int NumberOfLeds
        {
            get { return Width * Height; }
        }


        /// <summary>
        /// Gets the number of outputs required for the ledstrip.
        /// </summary>
        /// <value>
        /// The number of outputs of the ledstrip.
        /// </value>
        public int NumberOfOutputs
        {
            get { return NumberOfLeds * 3; }
        }


        private LedStripArrangementEnum _LedStripAranggement = LedStripArrangementEnum.LeftRightTopDown;

        /// <summary>
        /// Gets or sets the strip arrangement.
        /// The following image explains the meaining of the different values.
        /// 
        /// </summary>
        /// <value>
        /// The strip arrangement value as defined in the LedStripArrangementEnum.
        /// </value>
        public LedStripArrangementEnum LedStripArrangement
        {
            get { return _LedStripAranggement; }
            set { _LedStripAranggement = value; }
        }



        private RGBOrderEnum _ColorOrder = RGBOrderEnum.RBG;

        /// <summary>
        /// Gets or sets the order of the colors for the leds of the led strip.
        /// Usually colors are represented in RGB (Red - Green - Blue) order, but depending on the type of the used strip the color order might be different (e.g. WS2812 led chips have green - red - blue as their color order).
        /// </summary>
        /// <value>
        /// The color order of the leds on the strip.
        /// </value>
        public RGBOrderEnum ColorOrder
        {
            get { return _ColorOrder; }
            set { _ColorOrder = value; }
        }


        private int _FirstLedNumber = 1;
        /// <summary>
        /// Gets or sets the number of the first led of the strip.
        /// </summary>
        /// <value>
        /// The number of the first led of the strip.
        /// </value>
        public int FirstLedNumber
        {
            get { return _FirstLedNumber; }
            set { _FirstLedNumber = value.Limit(1, int.MaxValue); }
        }

        private string _FadingCurveName = "Linear";
        private Curve FadingCurve = null;

        /// <summary>
        /// Gets or sets the name of the fading curve as defined in the Curves list of the cabinet object.
        /// This curve can be used to adjust the brightness values for ther led to the brightness perception of the human eye.
        /// </summary>
        /// <value>
        /// The name of the fading curve.
        /// </value>
        public string FadingCurveName
        {
            get { return _FadingCurveName; }
            set { _FadingCurveName = value; }
        }

        private void InitFadingCurve(Cabinet Cabinet)
        {
            if (Cabinet.Curves.Contains(FadingCurveName))
            {
                FadingCurve = Cabinet.Curves[FadingCurveName];
            }
            else if (!FadingCurveName.IsNullOrWhiteSpace())
            {
                if (Enum.GetNames(typeof(Curve.CurveTypeEnum)).Contains(FadingCurveName))
                {
                    Curve.CurveTypeEnum T = Curve.CurveTypeEnum.Linear;
                    Enum.TryParse(FadingCurveName, out T);
                    FadingCurve = new Curve(T);
                }
                else
                {
                    FadingCurve = new Curve(Curve.CurveTypeEnum.Linear) { Name = FadingCurveName };
                    Cabinet.Curves.Add(FadingCurveName);
                }
            }
            else
            {
                FadingCurve = new Curve(Curve.CurveTypeEnum.Linear);
            }

        }


        /// <summary>
        /// Gets or sets the name of the output controller to be used.
        /// </summary>
        /// <value>
        /// The name of the output controller.
        /// </value>
        public string OutputControllerName { get; set; }

        private ISupportsSetValues OutputController;

        #endregion




        /// <summary>
        /// Gets the layers dictionary of the toy.
        /// </summary>
        /// <value>
        /// The layers dictionary of the toy.
        /// </value>
        [XmlIgnore]
        public LedStripLayerDictionary Layers { get; private set; }


        #region IToy methods

        Cabinet Cabinet;
        /// <summary>
        /// Initializes the toy.
        /// </summary>
        /// <param name="Cabinet"><see cref="Cabinet" /> object  to which the <see cref="IToy" /> belongs.</param>
        public override void Init(Cabinet Cabinet)
        {
            this.Cabinet = Cabinet;

            if (Cabinet.OutputControllers.Contains(OutputControllerName) && Cabinet.OutputControllers[OutputControllerName] is ISupportsSetValues)
            {
                OutputController = (ISupportsSetValues)Cabinet.OutputControllers[OutputControllerName];
            }

            BuildMappingTables();
            OutputData = new byte[NumberOfOutputs];
            InitFadingCurve(Cabinet);
        }



        /// <summary>
        /// Resets the toy. Turns all outputs off.
        /// </summary>
        public override void Reset()
        {
            OutputController.SetValues(FirstLedNumber * 3, new byte[NumberOfLeds]);
        }

        /// <summary>
        /// Updates the data of the assigned output controller
        /// </summary>
        public override void UpdateOutputs()
        {
            if (OutputController != null && Layers.Count > 0)
            {
                SetOutputData();
                OutputController.SetValues(NumberOfOutputs, OutputData);

            };
        }

        #endregion



        public void SetLayer(int LayerNr, RGBAData[,] LedData)
        {
            if (LedData.GetUpperBound(0) == Width - 1 && LedData.GetUpperBound(1) == Height - 1)
            {
                Layers[LayerNr] = LedData;
            }
        }


        
        //private int[,] LedMappingTable = new int[0, 0];
        private int[,] OutputMappingTable = new int[0, 0];

        private void BuildMappingTables()
        {
            //LedMappingTable = new int[Width, Height];
            OutputMappingTable = new int[Width, Height];
            bool FirstException = true;
            int LedNr = 0;

            for (int Y = 0; Y < Height; Y++)
            {
                for (int X = 0; X < Width; X++)
                {

                    switch (LedStripArrangement)
                    {
                        case LedStripArrangementEnum.LeftRightTopDown:
                            LedNr = (Y * Width) + X;
                            break;
                        case LedStripArrangementEnum.LeftRightBottomUp:
                            LedNr = ((Height - 1 - Y) * Width) + X;
                            break;
                        case LedStripArrangementEnum.RightLeftTopDown:
                            LedNr = (Y * Width) + (Width - 1 - X);
                            break;
                        case LedStripArrangementEnum.RightLeftBottomUp:
                            LedNr = ((Height - 1 - Y) * Width) + (Width - 1 - X);
                            break;
                        case LedStripArrangementEnum.TopDownLeftRight:
                            LedNr = X * Height + Y;
                            break;
                        case LedStripArrangementEnum.TopDownRightLeft:
                            LedNr = ((Width - 1 - X) * Height) + Y;
                            break;
                        case LedStripArrangementEnum.BottomUpLeftRight:
                            LedNr = (X * Height) + (Height - 1 - Y);
                            break;
                        case LedStripArrangementEnum.BottomUpRightLeft:
                            LedNr = ((Width - 1 - X) * Height) + (Height - 1 - Y);
                            break;
                        case LedStripArrangementEnum.LeftRightAlternateTopDown:
                            LedNr = (Width * Y) + ((Y & 1) == 0 ? X : (Width - 1 - X));
                            break;
                        case LedStripArrangementEnum.LeftRightAlternateBottomUp:
                            LedNr = (Width * (Height - 1 - Y)) + (((Height - 1 - Y) & 1) == 0 ? X : (Width - 1 - X));
                            break;
                        case LedStripArrangementEnum.RightLeftAlternateTopDown:
                            LedNr = (Width * Y) + ((Y & 1) == 1 ? X : (Width - 1 - X));
                            break;
                        case LedStripArrangementEnum.RightLeftAlternateBottomUp:
                            LedNr = (Width * (Height - 1 - Y)) + (((Height - 1 - Y) & 1) == 1 ? X : (Width - 1 - X));
                            break;
                        case LedStripArrangementEnum.TopDownAlternateLeftRight:
                            LedNr = (Height * X) + ((X & 1) == 0 ? Y : (Height - 1 - Y));
                            break;
                        case LedStripArrangementEnum.TopDownAlternateRightLeft:
                            LedNr = (Height * (Width - 1 - X)) + ((X & 1) == 1 ? Y : (Height - 1 - Y));
                            break;
                        case LedStripArrangementEnum.BottomUpAlternateLeftRight:
                            LedNr = (Height * X) + ((X & 1) == 1 ? Y : (Height - 1 - Y));
                            break;
                        case LedStripArrangementEnum.BottomUpAlternateRightLeft:
                            LedNr = (Height * (Width - 1 - X)) + ((X & 1) == 0 ? Y : (Height - 1 - Y));
                            break;
                        default:
                            if (FirstException)
                            {
                                Log.Exception("Unknow LedStripArrangement value ({0}) found. Will use LeftRightTopDown mapping as fallback.".Build(LedStripArrangement.ToString()));
                                FirstException = false;
                            };
                            LedNr = (Y * Width) + X;
                            break;
                    }
                    //LedMappingTable[X, Y] = LedNr;
                    OutputMappingTable[X, Y] = LedNr * 3;
                }
            }

        }

        //Array for output data is not in GetResultingValues to avoid reinitiaslisation of the array
        byte[] OutputData = new byte[0];

        /// <summary>
        /// Gets a array of bytes values re'presenting the data to be sent to the led strip.
        /// </summary>
        /// <returns></returns>
        private void SetOutputData()
        {
            if (Layers.Count > 0)
            {
                //Blend layers
                float[, ,] Value = new float[Width, Height, 3];

                foreach (KeyValuePair<int, RGBAData[,]> KV in Layers)
                {
                    RGBAData[,] D = KV.Value;

                    int Nr = 0;
                    for (int y = 0; y < Height; y++)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            int Alpha = D[x, y].Alpha.Limit(0, 255);
                            if (Alpha != 0)
                            {
                                Value[x, y, 0] = AlphaMappingTable.AlphaMapping[255 - Alpha, (int)Value[x, y, 0]] + AlphaMappingTable.AlphaMapping[Alpha, D[x, y].Red.Limit(0, 255)];
                                Value[x, y, 1] = AlphaMappingTable.AlphaMapping[255 - Alpha, (int)Value[x, y, 1]] + AlphaMappingTable.AlphaMapping[Alpha, D[x, y].Blue.Limit(0, 255)];
                                Value[x, y, 2] = AlphaMappingTable.AlphaMapping[255 - Alpha, (int)Value[x, y, 2]] + AlphaMappingTable.AlphaMapping[Alpha, D[x, y].Green.Limit(0, 255)];
                            }
                            Nr++;
                        }
                    }
                }


                //The following code mapps the led data to the outputs of the stripe
                byte[] FadingTable = FadingCurve.Data;
                switch (ColorOrder)
                {
                    case RGBOrderEnum.RBG:
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                int OutputNumber = OutputMappingTable[x, y];
                                OutputData[OutputNumber] = FadingTable[(int)Value[x, y, 0]];
                                OutputData[OutputNumber + 2] = FadingTable[(int)Value[x, y, 1]];
                                OutputData[OutputNumber + 1] = FadingTable[(int)Value[x, y, 2]];
                            }
                        }
                        break;
                    case RGBOrderEnum.GRB:
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                int OutputNumber = OutputMappingTable[x, y];
                                OutputData[OutputNumber + 1] = FadingTable[(int)Value[x, y, 0]];
                                OutputData[OutputNumber] = FadingTable[(int)Value[x, y, 1]];
                                OutputData[OutputNumber + 2] = FadingTable[(int)Value[x, y, 2]];
                            }
                        }
                        break;
                    case RGBOrderEnum.GBR:
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                int OutputNumber = OutputMappingTable[x, y];
                                OutputData[OutputNumber + 1] = FadingTable[(int)Value[x, y, 0]];
                                OutputData[OutputNumber + 2] = FadingTable[(int)Value[x, y, 1]];
                                OutputData[OutputNumber] = FadingTable[(int)Value[x, y, 2]];
                            }
                        }
                        break;
                    case RGBOrderEnum.BRG:
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                int OutputNumber = OutputMappingTable[x, y];
                                OutputData[OutputNumber + 2] = FadingTable[(int)Value[x, y, 0]];
                                OutputData[OutputNumber] = FadingTable[(int)Value[x, y, 1]];
                                OutputData[OutputNumber + 1] = FadingTable[(int)Value[x, y, 2]];
                            }
                        }
                        break;
                    case RGBOrderEnum.BGR:
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                int OutputNumber = OutputMappingTable[x, y];
                                OutputData[OutputNumber + 2] = FadingTable[(int)Value[x, y, 0]];
                                OutputData[OutputNumber + 1] = FadingTable[(int)Value[x, y, 1]];
                                OutputData[OutputNumber] = FadingTable[(int)Value[x, y, 2]];
                            }
                        }
                        break;
                    case RGBOrderEnum.RGB:
                    default:
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                int OutputNumber = OutputMappingTable[x, y];
                                OutputData[OutputNumber] = FadingTable[(int)Value[x, y, 0]];
                                OutputData[OutputNumber + 1] = FadingTable[(int)Value[x, y, 1]];
                                OutputData[OutputNumber + 2] = FadingTable[(int)Value[x, y, 2]];
                            }
                        }
                        break;
                }
            }

        }

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="LedStrip"/> class.
        /// </summary>
        public LedStrip()
        {
            Layers = new LedStripLayerDictionary();


        }
        #endregion

    }
}
