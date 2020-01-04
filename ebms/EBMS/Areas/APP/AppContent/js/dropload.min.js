///**
// * dropload
// * 西门(http://ons.me/526.html)
// * 0.9.1(161205)
// */
//!function(a){"use strict";function g(a){a.touches||(a.touches=a.originalEvent.touches)}function h(a,b){b._startY=a.touches[0].pageY,b.touchScrollTop=b.$scrollArea.scrollTop()}function i(b,c){c._curY=b.touches[0].pageY,c._moveY=c._curY-c._startY,c._moveY>0?c.direction="down":c._moveY<0&&(c.direction="up");var d=Math.abs(c._moveY);""!=c.opts.loadUpFn&&c.touchScrollTop<=0&&"down"==c.direction&&!c.isLockUp&&(b.preventDefault(),c.$domUp=a("."+c.opts.domUp.domClass),c.upInsertDOM||(c.$element.prepend('<div class="'+c.opts.domUp.domClass+'"></div>'),c.upInsertDOM=!0),n(c.$domUp,0),d<=c.opts.distance?(c._offsetY=d,c.$domUp.html(c.opts.domUp.domRefresh)):d>c.opts.distance&&d<=2*c.opts.distance?(c._offsetY=c.opts.distance+.5*(d-c.opts.distance),c.$domUp.html(c.opts.domUp.domUpdate)):c._offsetY=c.opts.distance+.5*c.opts.distance+.2*(d-2*c.opts.distance),c.$domUp.css({height:c._offsetY}))}function j(b){var c=Math.abs(b._moveY);""!=b.opts.loadUpFn&&b.touchScrollTop<=0&&"down"==b.direction&&!b.isLockUp&&(n(b.$domUp,300),c>b.opts.distance?(b.$domUp.css({height:b.$domUp.children().height()}),b.$domUp.html(b.opts.domUp.domLoad),b.loading=!0,b.opts.loadUpFn(b)):b.$domUp.css({height:"0"}).on("webkitTransitionEnd mozTransitionEnd transitionend",function(){b.upInsertDOM=!1,a(this).remove()}),b._moveY=0)}function k(a){""!=a.opts.loadDownFn&&a.opts.autoLoad&&a._scrollContentHeight-a._threshold<=a._scrollWindowHeight&&m(a)}function l(a){a._scrollContentHeight=a.opts.scrollArea==b?e.height():a.$element[0].scrollHeight}function m(a){a.direction="up",a.$domDown.html(a.opts.domDown.domLoad),a.loading=!0,a.opts.loadDownFn(a)}function n(a,b){a.css({"-webkit-transition":"all "+b+"ms",transition:"all "+b+"ms"})}var f,b=window,c=document,d=a(b),e=a(c);a.fn.dropload=function(a){return new f(this,a)},f=function(a,b){var c=this;c.$element=a,c.upInsertDOM=!1,c.loading=!1,c.isLockUp=!1,c.isLockDown=!1,c.isData=!0,c._scrollTop=0,c._threshold=0,c.init(b)},f.prototype.init=function(f){var l=this;l.opts=a.extend(!0,{},{scrollArea:l.$element,domUp:{domClass:"dropload-up",domRefresh:'<div class="dropload-refresh">↓下拉刷新</div>',domUpdate:'<div class="dropload-update">↑释放更新</div>',domLoad:'<div class="dropload-load"><span class="loading"></span>加载中...</div>'},domDown:{domClass:"dropload-down",domRefresh:'<div class="dropload-refresh">↑上拉加载更多</div>',domLoad:'<div class="dropload-load"><span class="loading"></span>加载中...</div>',domNoData:'<div class="dropload-noData">暂无数据</div>'},autoLoad:!0,distance:50,threshold:"",loadUpFn:"",loadDownFn:""},f),""!=l.opts.loadDownFn&&(l.$element.append('<div class="'+l.opts.domDown.domClass+'">'+l.opts.domDown.domRefresh+"</div>"),l.$domDown=a("."+l.opts.domDown.domClass)),l._threshold=l.$domDown&&""===l.opts.threshold?Math.floor(1*l.$domDown.height()/3):l.opts.threshold,l.opts.scrollArea==b?(l.$scrollArea=d,l._scrollContentHeight=e.height(),l._scrollWindowHeight=c.documentElement.clientHeight):(l.$scrollArea=l.opts.scrollArea,l._scrollContentHeight=l.$element[0].scrollHeight,l._scrollWindowHeight=l.$element.height()),k(l),d.on("resize",function(){clearTimeout(l.timer),l.timer=setTimeout(function(){l._scrollWindowHeight=l.opts.scrollArea==b?b.innerHeight:l.$element.height(),k(l)},150)}),l.$element.on("touchstart",function(a){l.loading||(g(a),h(a,l))}),l.$element.on("touchmove",function(a){l.loading||(g(a,l),i(a,l))}),l.$element.on("touchend",function(){l.loading||j(l)}),l.$scrollArea.on("scroll",function(){l._scrollTop=l.$scrollArea.scrollTop(),""!=l.opts.loadDownFn&&!l.loading&&!l.isLockDown&&l._scrollContentHeight-l._threshold<=l._scrollWindowHeight+l._scrollTop&&m(l)})},f.prototype.lock=function(a){var b=this;void 0===a?"up"==b.direction?b.isLockDown=!0:"down"==b.direction?b.isLockUp=!0:(b.isLockUp=!0,b.isLockDown=!0):"up"==a?b.isLockUp=!0:"down"==a&&(b.isLockDown=!0,b.direction="up")},f.prototype.unlock=function(){var a=this;a.isLockUp=!1,a.isLockDown=!1,a.direction="up"},f.prototype.noData=function(a){var b=this;void 0===a||1==a?b.isData=!1:0==a&&(b.isData=!0)},f.prototype.resetload=function(){var b=this;"down"==b.direction&&b.upInsertDOM?b.$domUp.css({height:"0"}).on("webkitTransitionEnd mozTransitionEnd transitionend",function(){b.loading=!1,b.upInsertDOM=!1,a(this).remove(),l(b)}):"up"==b.direction&&(b.loading=!1,b.isData?(b.$domDown.html(b.opts.domDown.domRefresh),l(b),k(b)):b.$domDown.html(b.opts.domDown.domNoData))}}(window.Zepto||window.jQuery);
/**
 * dropload
 * 西门(http://ons.me/526.html)
 * 0.9.1(161205)
 */

;(function($){
    'use strict';
    var win = window;
    var doc = document;
    var $win = $(win);
    var $doc = $(doc);
    $.fn.dropload = function(options){
        return new MyDropLoad(this, options);
    };
    var MyDropLoad = function(element, options){
        var me = this;
        me.$element = element;
        // 上方是否插入DOM
        me.upInsertDOM = false;
        // loading状态
        me.loading = false;
        // 是否锁定
        me.isLockUp = false;
        me.isLockDown = false;
        // 是否有数据
        me.isData = true;
        me._scrollTop = 0;
        me._threshold = 0;
        me.init(options);
    };

    // 初始化
    MyDropLoad.prototype.init = function(options){
        var me = this;
        me.opts = $.extend(true, {}, {
            scrollArea : me.$element,                                            // 滑动区域
            domUp : {                                                            // 上方DOM
                domClass   : 'dropload-up',
                domRefresh : '<div class="dropload-refresh">↓下拉刷新</div>',
                domUpdate  : '<div class="dropload-update">↑释放更新</div>',
                domLoad    : '<div class="dropload-load"><span class="loading"></span>加载中...</div>'
            },
            domDown : {                                                          // 下方DOM
                domClass   : 'dropload-down',
                domRefresh : '<div class="dropload-refresh">↑上拉加载更多</div>',
                domLoad    : '<div class="dropload-load"><span class="loading"></span>加载中...</div>',
                domNoData  : '<div class="dropload-noData">暂无数据</div>'
            },
            autoLoad : true,                                                     // 自动加载
            distance : 50,                                                       // 拉动距离
            threshold : '',                                                      // 提前加载距离
            loadUpFn : '',                                                       // 上方function
            loadDownFn : ''                                                      // 下方function
        }, options);

        // 如果加载下方，事先在下方插入DOM
        if(me.opts.loadDownFn != ''){
            me.$element.append('<div class="'+me.opts.domDown.domClass+'">'+me.opts.domDown.domRefresh+'</div>');
            me.$domDown = $('.'+me.opts.domDown.domClass);
        }

        // 计算提前加载距离
        if(!!me.$domDown && me.opts.threshold === ''){
            // 默认滑到加载区2/3处时加载
            me._threshold = Math.floor(me.$domDown.height()*1/3);
        }else{
            me._threshold = me.opts.threshold;
        }

        // 判断滚动区域
        if(me.opts.scrollArea == win){
            me.$scrollArea = $win;
            // 获取文档高度
            me._scrollContentHeight = $doc.height();
            // 获取win显示区高度  —— 这里有坑
            me._scrollWindowHeight = doc.documentElement.clientHeight;
        }else{
            me.$scrollArea = me.opts.scrollArea;
            me._scrollContentHeight = me.$element[0].scrollHeight;
            me._scrollWindowHeight = me.$element.height();
        }
        fnAutoLoad(me);

        // 窗口调整
        $win.on('resize',function(){
            clearTimeout(me.timer);
            me.timer = setTimeout(function(){
                if(me.opts.scrollArea == win){
                // 重新获取win显示区高度
                me._scrollWindowHeight = win.innerHeight;
                }else{
                    me._scrollWindowHeight = me.$element.height();
                }
                fnAutoLoad(me);
            },150);
            
        });

        // 绑定触摸
        me.$element.on('touchstart',function(e){
            if(!me.loading){
                fnTouches(e);
                fnTouchstart(e, me);
            }
        });
        me.$element.on('touchmove',function(e){
            if(!me.loading){
                fnTouches(e, me);
                fnTouchmove(e, me);
            }
        });
        me.$element.on('touchend',function(){
            if(!me.loading){
                fnTouchend(me);
            }
        });

        // 加载下方
        me.$scrollArea.on('scroll',function(){
            me._scrollTop = me.$scrollArea.scrollTop();

            // 滚动页面触发加载数据
            if(me.opts.loadDownFn != '' && !me.loading && !me.isLockDown && (me._scrollContentHeight - me._threshold) <= (me._scrollWindowHeight + me._scrollTop)){
                loadDown(me);
            }
        });
    };

    // touches
    function fnTouches(e){
        if(!e.touches){
            e.touches = e.originalEvent.touches;
        }
    }

    // touchstart
    function fnTouchstart(e, me){
        me._startY = e.touches[0].pageY;
        // 记住触摸时的scrolltop值
        me.touchScrollTop = me.$scrollArea.scrollTop();
    }

    // touchmove
    function fnTouchmove(e, me){
        me._curY = e.touches[0].pageY;
        me._moveY = me._curY - me._startY;

        if(me._moveY > 0){
            me.direction = 'down';
        }else if(me._moveY < 0){
            me.direction = 'up';
        }

        var _absMoveY = Math.abs(me._moveY);

        // 加载上方
        if(me.opts.loadUpFn != '' && me.touchScrollTop <= 0 && me.direction == 'down' && !me.isLockUp){
            e.preventDefault();

            me.$domUp = $('.'+me.opts.domUp.domClass);
            // 如果加载区没有DOM
            if(!me.upInsertDOM){
                me.$element.prepend('<div class="'+me.opts.domUp.domClass+'"></div>');
                me.upInsertDOM = true;
            }
            
            fnTransition(me.$domUp,0);

            // 下拉
            if(_absMoveY <= me.opts.distance){
                me._offsetY = _absMoveY;
                // todo：move时会不断清空、增加dom，有可能影响性能，下同
                me.$domUp.html(me.opts.domUp.domRefresh);
            // 指定距离 < 下拉距离 < 指定距离*2
            }else if(_absMoveY > me.opts.distance && _absMoveY <= me.opts.distance*2){
                me._offsetY = me.opts.distance+(_absMoveY-me.opts.distance)*0.5;
                me.$domUp.html(me.opts.domUp.domUpdate);
            // 下拉距离 > 指定距离*2
            }else{
                me._offsetY = me.opts.distance+me.opts.distance*0.5+(_absMoveY-me.opts.distance*2)*0.2;
            }

            me.$domUp.css({'height': me._offsetY});
        }
    }

    // touchend
    function fnTouchend(me){
   
   
        var _absMoveY = Math.abs(me._moveY);
        if(me.opts.loadUpFn != '' && me.touchScrollTop <= 0 && me.direction == 'down' && !me.isLockUp){
            fnTransition(me.$domUp,300);

            if(_absMoveY > me.opts.distance){
             if(me.$domUp){
             me.$domUp.css({'height':me.$domUp.children().height()});
                me.$domUp.html(me.opts.domUp.domLoad);
                me.loading = true;
                me.opts.loadUpFn(me);
             }
                
            }else{
            if(me.$domUp){
             me.$domUp.css({'height':'0'}).on('webkitTransitionEnd mozTransitionEnd transitionend',function(){
                    me.upInsertDOM = false;
                    $(this).remove();
                });
            }
            me._moveY = 0;
            }
               
        }
    }

    // 如果文档高度不大于窗口高度，数据较少，自动加载下方数据
    function fnAutoLoad(me){
        if(me.opts.loadDownFn != '' && me.opts.autoLoad){
            if((me._scrollContentHeight - me._threshold) <= me._scrollWindowHeight){
                loadDown(me);
            }
        }
    }

    // 重新获取文档高度
    function fnRecoverContentHeight(me){
        if(me.opts.scrollArea == win){
            me._scrollContentHeight = $doc.height();
        }else{
            me._scrollContentHeight = me.$element[0].scrollHeight;
        }
    }

    // 加载下方
    function loadDown(me){
        me.direction = 'up';
        me.$domDown.html(me.opts.domDown.domLoad);
        me.loading = true;
        me.opts.loadDownFn(me);
    }

    // 锁定
    MyDropLoad.prototype.lock = function(direction){
        var me = this;
        // 如果不指定方向
        if(direction === undefined){
            // 如果操作方向向上
            if(me.direction == 'up'){
                me.isLockDown = true;
            // 如果操作方向向下
            }else if(me.direction == 'down'){
                me.isLockUp = true;
            }else{
                me.isLockUp = true;
                me.isLockDown = true;
            }
        // 如果指定锁上方
        }else if(direction == 'up'){
            me.isLockUp = true;
        // 如果指定锁下方
        }else if(direction == 'down'){
            me.isLockDown = true;
            // 为了解决DEMO5中tab效果bug，因为滑动到下面，再滑上去点tab，direction=down，所以有bug
            me.direction = 'up';
        }
    };

    // 解锁
    MyDropLoad.prototype.unlock = function(){
        var me = this;
        // 简单粗暴解锁
        me.isLockUp = false;
        me.isLockDown = false;
        // 为了解决DEMO5中tab效果bug，因为滑动到下面，再滑上去点tab，direction=down，所以有bug
        me.direction = 'up';
    };

    // 无数据
    MyDropLoad.prototype.noData = function(flag){
        var me = this;
        if(flag === undefined || flag == true){
            me.isData = false;
        }else if(flag == false){
            me.isData = true;
        }
    };

    // 重置
    MyDropLoad.prototype.resetload = function(){
        var me = this;
        if(me.direction == 'down' && me.upInsertDOM){
            me.$domUp.css({'height':'0'}).on('webkitTransitionEnd mozTransitionEnd transitionend',function(){
                me.loading = false;
                me.upInsertDOM = false;
                $(this).remove();
                fnRecoverContentHeight(me);
            });
        }else if(me.direction == 'up'){
            me.loading = false;
            // 如果有数据
            if(me.isData){
                // 加载区修改样式
                me.$domDown.html(me.opts.domDown.domRefresh);
                fnRecoverContentHeight(me);
                fnAutoLoad(me);
            }else{
                // 如果没数据
                me.$domDown.html(me.opts.domDown.domNoData);
            }
        }
    };

    // css过渡
    function fnTransition(dom,num){
      if(dom){
	      dom.css({
	            '-webkit-transition':'all '+num+'ms',
	            'transition':'all '+num+'ms'
	        });
      }
        
    }
})(window.Zepto || window.jQuery);
