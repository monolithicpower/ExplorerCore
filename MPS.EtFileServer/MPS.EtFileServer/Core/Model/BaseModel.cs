using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MPS.EtFileServer.Core.Model
{
    public class BaseModel
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTimeOffset? CrtTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public virtual DateTimeOffset? UpdTime { get; set; }

        /// <summary>
        /// 创建者名称
        /// </summary>
        public virtual string CrtUserName { get; set; }

        /// <summary>
        /// 修改者名称
        /// </summary>
        public virtual string UpdUserName { get; set; }

        /// <summary>
        /// 软删除
        /// </summary>
        public virtual bool IsDeleted { get; set; } = false;
    }
}
