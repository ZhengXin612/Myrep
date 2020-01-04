
      $(function () {
          $('#tab_menu-tabrefresh').click(function () {
              /*重新设置该标签 */
              var url = $(".tabs-panels .panel").eq($('.tabs-selected').index()).find("iframe").attr("src");
              $(".tabs-panels .panel").eq($('.tabs-selected').index()).find("iframe").attr("src", url);
          });
          //在新窗口打开该标签
          $('#tab_menu-openFrame').click(function () {
              var url = $(".tabs-panels .panel").eq($('.tabs-selected').index()).find("iframe").attr("src");
              window.open(url);
          });
          //关闭当前
          $('#tab_menu-tabclose').click(function () {
              var currtab_title = $('.tabs-selected .tabs-inner span').text();
              $('#mainTab').tabs('close', currtab_title);
              if ($(".tabs li").length == 0) {
                  //open menu
                  $(".layout-button-right").trigger("click");
              }
          });
          //全部关闭
          $('#tab_menu-tabcloseall').click(function () {
              $('.tabs-inner span').each(function (i, n) {
                  if ($(this).parent().next().is('.tabs-close')) {
                      var t = $(n).text();
                      $('#mainTab').tabs('close', t);
                  }
              });
              //open menu
              $(".layout-button-right").trigger("click");
          });
          //关闭除当前之外的TAB
          $('#tab_menu-tabcloseother').click(function () {
              var currtab_title = $('.tabs-selected .tabs-inner span').text();
              $('.tabs-inner span').each(function (i, n) {
                  if ($(this).parent().next().is('.tabs-close')) {
                      var t = $(n).text();
                      if (t != currtab_title)
                          $('#mainTab').tabs('close', t);
                  }
              });
          });
          //关闭当前右侧的TAB
          $('#tab_menu-tabcloseright').click(function () {
              var nextall = $('.tabs-selected').nextAll();
              if (nextall.length == 0) {
                  $.messager.alert('提示', '前面没有了!', 'warning');
                  return false;
              }
              nextall.each(function (i, n) {
                  if ($('a.tabs-close', $(n)).length > 0) {
                      var t = $('a:eq(0) span', $(n)).text();
                      $('#mainTab').tabs('close', t);
                  }
              });
              return false;
          });
          //关闭当前左侧的TAB
          $('#tab_menu-tabcloseleft').click(function () {

              var prevall = $('.tabs-selected').prevAll();
              if (prevall.length == 0) {
                  $.messager.alert('提示', '后面没有了!', 'warning');
                  return false;
              }
              prevall.each(function (i, n) {
                  if ($('a.tabs-close', $(n)).length > 0) {
                      var t = $('a:eq(0) span', $(n)).text();
                      $('#mainTab').tabs('close', t);
                  }
              });
              return false;
          });

      });
$(function () {
    /*为选项卡绑定右键*/
    getContextmenu()
});
function getContextmenu() {
    $(".tabs li").bind('contextmenu', function (e) {
        /*选中当前触发事件的选项卡 */
        var subtitle = $(this).text();
        $('#mainTab').tabs('select', subtitle);
        //显示快捷菜单
        $('#tab_menu').menu('show', {
            left: e.pageX,
            top: e.pageY
        });
        return false;
    });
}
function addTab(subtitle, url, icon) {
    if (!$("#mainTab").tabs('exists', subtitle)) {
        $("#mainTab").tabs('add', {
            title: subtitle,
            content: createFrame(url),
            closable: true,
            icon: icon
        });
    } else {
        $("#mainTab").tabs('select', subtitle);
        $("#tab_menu-tabrefresh").trigger("click");
    }
    //mike 左侧 $(".layout-button-left").trigger("click");
    //tabClose();
}
function createFrame(url) {
    var s = '<iframe frameborder="0" src="' + url + '" scrolling="auto" style="width:100%; height:99%"></iframe>';
    return s;
}
function closeCurrTab() {
   
    var currtab_title = $('.tabs-selected .tabs-inner span').text();
    $('#mainTab').tabs('close', currtab_title);
}
function closeTab(title) {
    
    $('#mainTab').tabs('close', title);
}

function refreshTab(title) {
    if ($('#mainTab').tabs('exists', title)) {
        var currTab= $('#mainTab').tabs('getTab', title);//选中并刷新
        var url = $(currTab.panel('options').content).attr('src'); 
        $('#mainTab').tabs('update', {
            tab: currTab,     
            options: {
                content: createFrame(url)
            }
        })
    }
}
    $(function () {
        $(".ui-skin-nav .li-skinitem span").click(function () {
            var theme = $(this).attr("rel");
            $.messager.confirm('提示', '切换皮肤将重新加载系统！', function (r) {
                if (r) {
                    $.post("/Home/SetThemes", { value: theme }, function (data) { window.location.reload(); }, "json");
                }
            });
        });
    });

    function enterSearch(btnSearch) {
      
        var SubmitOrHidden = function (evt) {
            evt = window.event || evt;
            if (evt.keyCode == 13) {//如果取到的键值是回车
                var btn = $(btnSearch);
                if(btn!=null)
                    btn.click();//页面查询方法
            }
           
        }
        window.document.onkeydown = SubmitOrHidden;//当有键按下时执行函数
    }
//判断查询条件是否为空
    function isnull(data) {
        if (data == null || data == "") {
            return "";
        }
        else {
            return data;
        }
    }
    // 转换货币格式，带小数2位
    function toMoeny(val) {
        if (val == null) {
            return 0.00;
        }
        var s = val; //获取小数型数据
        s += "";
        if (s.indexOf(".") == -1) s += ".0"; //如果没有小数点，在后面补个小数点和0
        if (/\.\d$/.test(s)) s += "0";   //正则判断


        if (s == "0.00" || s == ".00") {
            return "";
        }
        while (/\d{4}(\.|,)/.test(s))  //符合条件则进行替换
            s = s.replace(/(\d)(\d{3}(\.|,))/, "$1,$2"); //每隔3位添加一个
        return s;
    }
//点击保存的时候 弹出正在处理的方法   status参数  0=>显示 1=>关闭

    function loading(status) {
        if (status == 0) {
            $("body").append("<div class='loading-box'>系统正在处理</div>")
        }
        else {
            $(".loading-box").remove();
        }

    }
    //编辑成功关闭 刷新数据列表
    function closeReshefTab(modelName) {
        //获取编辑tabs的index
        var editTab = self.parent.$('#mainTab').tabs('getSelected');
        var index = self.parent.$('#mainTab').tabs('getTabIndex', editTab);
        //刷新管理页面tabs
        var parentTab = parent.$('#mainTab').tabs('select', modelName);
        var currTab = self.parent.$('#mainTab').tabs('getSelected'); //获得当前tab
        var url = $(currTab.panel('options').content).attr('src');
        self.parent.$('#mainTab').tabs('update', {
            tab: currTab,
            options: {
                content: createFrame(url)
            }
        });
        //根据index关闭 编辑tabs
        self.parent.$('#mainTab').tabs('close', index);
    }