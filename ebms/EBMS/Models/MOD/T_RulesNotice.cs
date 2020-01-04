using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EBMS.Models
{
      [MetadataType(typeof(MOD))]
    public partial class T_RulesNotice
    {
          class MOD
          {
              [Display(Name = "主键")]
              public int ID { get; set; }
              [Display(Name = "标题")]
              public string Title { get; set; }
               [Display(Name = "摘要")]
              public string Summary { get; set; }
               [Display(Name = "创建时间")]
              public System.DateTime CreateDate { get; set; }
              [Display(Name = "创建人")]
              public string CreateName { get; set; }
               [Display(Name = "内容")]
              public string Contents { get; set; }
              [Display(Name = "谁可以看")]
              public string Power { get; set; }
               [Display(Name = "类型")]
              public string Type { get; set; }
          }
    }
}