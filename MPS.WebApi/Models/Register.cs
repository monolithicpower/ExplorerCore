using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MPS.WebApi.Models
{
    [Serializable]
    public class Register
    {
        [XmlAttribute]
        [Description("ID")]
        public int Id { get; set; }

        #region calc property
        [XmlAttribute]
        public int EqId { get; set; }
        #endregion

        #region READ/WRITE property
        private string name;
        [XmlAttribute]
        [Description("寄存器名称")]
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        private byte page;
        [XmlAttribute]
        [Description("寄存器的Page信息")]
        public byte Page
        {
            get { return page; }
            set
            {
                if (this.page != value)
                {
                    page = value;
                    OnPropertyChanged(() => Page);
                }
            }
        }

        private byte address;
        [XmlAttribute]
        [Description("寄存器地址-数字")]
        public byte Address
        {
            get { return address; }
            set
            {
                if (this.address != value)
                {
                    address = value;
                    OnPropertyChanged(() => Address);
                }
            }
        }

        private int byteCount;
        [XmlAttribute]
        [Description("寄存器在RAM中的Byte长度")]
        public int ByteCount
        {
            get { return byteCount; }
            set
            {
                if (this.byteCount != value)
                {
                    byteCount = value;
                    OnPropertyChanged(() => ByteCount);
                }
            }
        }

        private bool isBlock;
        [XmlAttribute]
        [Description("寄存器是否使用block方式进行读写/操作")]
        public bool IsBlock
        {
            get { return isBlock; }
            set
            {
                if (this.isBlock != value)
                {
                    isBlock = value;
                    OnPropertyChanged(() => IsBlock);
                }
            }
        }

        private bool hasRom;
        [XmlAttribute]
        [Description("寄存器是否使用block方式进行读写/操作")]
        public bool HasRom
        {
            get { return hasRom; }
            set
            {
                if (this.hasRom != value)
                {
                    hasRom = value;
                    OnPropertyChanged(() => HasRom);
                }
            }
        }

        #endregion

        #region Mtp/Rom Property
        private int bitsCount;
        [XmlAttribute]
        [Description("寄存器在ROM中的Bit长度")]
        public int BitsCount
        {
            get { return bitsCount; }
            set
            {
                if (this.bitsCount != value)
                {
                    bitsCount = value;
                    OnPropertyChanged(() => BitsCount);
                }
            }
        }

        private int mtpAddress;
        [XmlAttribute]
        public int MtpAddress
        {
            get { return mtpAddress; }
            set
            {
                if (this.mtpAddress != value)
                {
                    mtpAddress = value;
                    OnPropertyChanged(() => MtpAddress);
                }
            }
        }
        #endregion

        #region 业务属性

        private string category;
        [XmlAttribute]
        [Description("寄存器所属类型")]
        public string Category
        {
            get { return category; }
            set { SetProperty(ref category, value); }
        }

        private bool isUpdated;
        [XmlIgnore]
        [Description("当因公式计算产生修改时置为true，用以界面显示蓝色，update至芯片时置为false")]
        public bool IsUpdated
        {
            get { return isUpdated; }
            set
            {
                if (this.isUpdated != value)
                {
                    isUpdated = value;
                    OnPropertyChanged(() => IsUpdated);
                }
            }
        }

        private bool isSelected;
        [XmlIgnore]
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (this.isSelected != value)
                {
                    isSelected = value;
                    OnPropertyChanged(() => IsSelected);
                }
            }
        }

        private string description;
        [XmlAttribute]
        [Description("寄存器的相关描述")]
        public string Description
        {
            get { return description; }
            set { SetProperty(ref this.description, value); }
        }

        private bool isExport;
        [XmlAttribute]
        [Description("寄存器在GEN3中是否需要导出")]
        public bool IsExport
        {
            get { return isExport; }
            set
            {
                if (this.isExport != value)
                {
                    isExport = value;
                    OnPropertyChanged(() => IsExport);
                }
            }
        }

        private bool isDsExport;
        [XmlAttribute]
        [Description("寄存器在导出Ds时是否需要导出")]
        public bool IsDsExport
        {
            get { return isDsExport; }
            set
            {
                if (this.isDsExport != value)
                {
                    isDsExport = value;
                    OnPropertyChanged(() => IsDsExport);
                }
            }
        }

        private bool isMonitor;
        [XmlAttribute]
        [Description("寄存器是否属于Monitor")]
        public bool IsMonitor
        {
            get { return isMonitor; }
            set
            {
                if (this.isMonitor != value)
                {
                    isMonitor = value;
                    OnPropertyChanged(() => IsMonitor);
                }
            }
        }

        private int fileValue;
        /// <summary>
        /// 文件值，读取excel文件后赋值，作为界面值的缓冲存在
        /// </summary>
        [XmlIgnore]
        public int FileValue
        {
            get { return fileValue; }
            set
            {
                if (this.fileValue != value)
                {
                    this.fileValue = value;
                    OnPropertyChanged(() => FileValue);
                }
            }
        }

        #endregion

        [field: NonSerializedAttribute()]
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals((object)storage, (object)value))
                return false;
            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            this.OnPropertyChanged(PropertySupport.ExtractPropertyName<T>(propertyExpression));
        }
    }
    public static class PropertySupport
    {
        public static string ExtractPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException(nameof(propertyExpression));
            if (!(propertyExpression.Body is MemberExpression))
                throw new ArgumentException("请传入类的属性成员", nameof(propertyExpression));
            var body = propertyExpression.Body as MemberExpression;
            if (!(body.Member is PropertyInfo))
                throw new ArgumentException("请传入类的属性成员", nameof(propertyExpression));
            var member = body.Member as PropertyInfo;
            if (member.GetMethod.IsStatic)
                throw new ArgumentException("请传入类的属性非静态成员", nameof(propertyExpression));
            return member.Name;
        }
    }
}
