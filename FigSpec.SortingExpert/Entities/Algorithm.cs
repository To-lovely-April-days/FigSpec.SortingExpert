using FigSpec.SortingExpert.Tools;
using Globalization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Entities
{

    public class Algorithm : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        //算法类型
        [CategoryExtend("常规"), DisplayNameExtend("算法类型"), ReadOnly(false), Browsable(false)]
        public TypeAlgorithm TypeAlgorithm { get; set; }
        /// <summary>
        /// 模型参数   pls-da
        /// </summary>
        //主成分数量
        [CategoryExtend("常规"), DisplayNameExtend("主成分数量(2-10)"), ReadOnly(false), Browsable(false)]
        public int n_components { get; set; } = 2;

        //验证集大小 0 - 1
        [CategoryExtend("常规"), DisplayNameExtend("训练集大小(0.5-1)"), ReadOnly(false), Browsable(false)]
        public float train_size { get; set; } = 0.8f;


        private int _startBandIndex = 0;
        [CategoryExtend("常规"), DisplayNameExtend("有效起始波长"), ReadOnly(false), Browsable(true)]
        [TypeConverter(typeof(BandsConverter))]
        public int StartBandIndex
        {
            get { return _startBandIndex; }
            set
            {
                if (_startBandIndex != value && value <= _endBandIndex)
                {
                    _startBandIndex = value;
                    OnPropertyChanged("StartBandIndex");
                }
                else if(_startBandIndex == _endBandIndex && _startBandIndex != value) 
                {
                    //序列化的时候
                    _startBandIndex = value;
                    _endBandIndex = value;
                    OnPropertyChanged("StartBandIndex");
                }
            }
        }

        private int _endBandIndex = 0;
        [CategoryExtend("常规"), DisplayNameExtend("有效结束波长"), ReadOnly(false), Browsable(true)]
        [TypeConverter(typeof(BandsConverter))]
        public int EndBandIndex
        {
            get { return _endBandIndex; }
            set
            {
                if (_endBandIndex != value && value >= _startBandIndex)
                {
                    _endBandIndex = value;
                    OnPropertyChanged("EndBandIndex");
                }
            }
        }

        private TrainBands _train_bands;
        //训练的波长集合
        [CategoryExtend("常规"), DisplayNameExtend("训练的波长集合"), ReadOnly(false), Browsable(true)]
        [Editor(typeof(TrainBandsEditor), typeof(UITypeEditor))]
        public TrainBands train_bands
        {
            get { return _train_bands; }
            set
            {
                if (_train_bands != value)
                {
                    _train_bands = value;
                    OnPropertyChanged("train_bands");
                }
            }
        }

        private bool _train_contour_enabled = false;
        [CategoryExtend("常规"), DisplayNameExtend("是否启用轮廓分选"), ReadOnly(false), Browsable(true)]
        [TypeConverter(typeof(BoolConverter))]
        public bool train_contour_enabled
        {
            get { return _train_contour_enabled; }
            set
            {
                if (_train_contour_enabled != value)
                {
                    _train_contour_enabled = value;
                    OnPropertyChanged("train_contour_enabled");
                }
            }
        }


        private float _train_contour_threshold = 100;
        [CategoryExtend("常规"), DisplayNameExtend("轮廓阈值(0-255)"), ReadOnly(false), Browsable(true)]
        public float train_contour_threshold
        {
            get { return _train_contour_threshold; }
            set
            {
                if (_train_contour_threshold != value)
                {
                    _train_contour_threshold = value;
                    OnPropertyChanged("train_contour_threshold");
                }
            }
        }

        private int _train_contour_erode { get; set; }
        [CategoryExtend("常规"), DisplayNameExtend("轮廓腐蚀度(0-100)"), ReadOnly(false), Browsable(true)]
        public int train_contour_erode
        {
            get { return _train_contour_erode; }
            set
            {
                if (_train_contour_erode != value)
                {
                    _train_contour_erode = value;
                    OnPropertyChanged("train_contour_erode");
                }
            }
        }

        private int _split_point_count = 100;
        [CategoryExtend("常规"), DisplayNameExtend("轮廓取点数量"), ReadOnly(false), Browsable(true)]
        public int split_point_count
        {
            get { return _split_point_count; }
            set
            {
                if (_split_point_count != value)
                {
                    _split_point_count = value;
                    OnPropertyChanged("split_point_count");
                }
            }
        }

        /// <summary>
        /// 背景启用
        /// </summary>
        public bool _backgroundCorrection = true;
        [CategoryExtend("常规"), DisplayNameExtend("是否启用扣除背景"), ReadOnly(false), Browsable(true)]
        [TypeConverter(typeof(BoolConverter))]
        public bool BackgroundCorrection
        {
            get { return _backgroundCorrection; }
            set
            {
                if (_backgroundCorrection != value)
                {
                    _backgroundCorrection = value;
                    OnPropertyChanged("BackgroundCorrection");
                }
            }
        }

        public float _startBackgroundThreshold = 0f;
        [CategoryExtend("常规"), DisplayNameExtend("背景反射率范围起始值(0~1)"), ReadOnly(false), Browsable(true)]
        public float StartBackgroundThreshold
        {
            get { return _startBackgroundThreshold; }
            set
            {
                if (_startBackgroundThreshold != value)
                {
                    _startBackgroundThreshold = value;
                    OnPropertyChanged("StartBackgroundThreshold");
                }
            }
        }


        public float _endBackgroundThreshold = 0.15f;
        [CategoryExtend("常规"), DisplayNameExtend("背景反射率范围结束值(0~1)"), ReadOnly(false), Browsable(true)]
        public float EndBackgroundThreshold
        {
            get { return _endBackgroundThreshold; }
            set
            {
                if (_endBackgroundThreshold != value && value >= _startBackgroundThreshold)
                {
                    _endBackgroundThreshold = value;
                    OnPropertyChanged("EndBackgroundThreshold");
                }
            }
        }

    }


    public enum TypeAlgorithm
    {
        PLSDA = 0,
        //SVM = 1,
        //OpenCV_SVM
    }


    public class TrainBands 
    {
        /// <summary>
        /// 显示到训练界面
        /// </summary>
        public bool ShowToTrainForm { get; set; }

        /// <summary>
        /// 使用相同波段
        /// </summary>
        public bool SelectSameBand { get; set; }

        /// <summary>
        /// 波长
        /// </summary>
        public float[] WaveLength { get; set; }

        /// <summary>
        /// Bands 中找不到，算全选波长
        /// </summary>
        public List<TrainBand> Bands { get; set; } = new List<TrainBand>();

        public override string ToString()
        {
            return "波长集合".ToMultiLanguage();
        }

    }

    public class TrainBand
    {
        /// <summary>
        /// 标签ID
        /// </summary>
        public int ClassId { get; set; }
        /// <summary>
        /// 选定的波长索引
        /// </summary>
        public List<int> WaveLengthIndexs { get; set; } = new List<int>();
    }
}
