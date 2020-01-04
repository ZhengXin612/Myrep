
function formatDatebox(value) {
    if (value == null || value == '') {
        return '';
    }
    var dt = parseToDate(value);//关键代码，将那个长字符串的日期值转换成正常的JS日期格式
    return dt.format("yyyy-MM-dd"); //这里用到一个javascript的Date类型的拓展方法，这个是自己添加的拓展方法，在后面的步骤3定义
}

function formatDatenotyyyy(value) {
    if (value == null || value == '') {
        return '';
    }
    var dt = parseToDate(value);//关键代码，将那个长字符串的日期值转换成正常的JS日期格式
    return dt.format("MM-dd"); //这里用到一个javascript的Date类型的拓展方法，这个是自己添加的拓展方法，在后面的步骤3定义
}
/*带时间*/
function formatDateBoxFull(value) {
    if (value == null || value == '') {
        return '';
    }
    var dt = parseToDate(value);
    return dt.format("yyyy-MM-dd hh:mm:ss");
}
/*年月日*/
function formatDateBoxFullnianyueri(value) {
    if (value == null || value == '') {
        return '';
    }
    var dt = parseToDate(value);
    return dt.format("yyyy年MM月dd日");
}
function parseToDate(value) {
    if (value == null || value == '') {
        return undefined;
    }

    var dt;
    if (value instanceof Date) {
        dt = value;
    }
    else {
        if (!isNaN(value)) {
            dt = new Date(value);
        }
        else if (value.indexOf('/Date') > -1) {
            value = value.replace(/\/Date\((-?\d+)\)\//, '$1');
            dt = new Date();
            dt.setTime(value);
        } else if (value.indexOf('/') > -1) {
            dt = new Date(Date.parse(value.replace(/-/g, '/')));
        } else {
            dt = new Date(value);
        }
    }
    return dt;
}

//为Date类型拓展一个format方法，用于格式化日期
Date.prototype.format = function (format) //author: meizz 
{
    var o = {
        "M+": this.getMonth() + 1, //month 
        "d+": this.getDate(),    //day 
        "h+": this.getHours(),   //hour 
        "m+": this.getMinutes(), //minute 
        "s+": this.getSeconds(), //second 
        "q+": Math.floor((this.getMonth() + 3) / 3),  //quarter 
        "S": this.getMilliseconds() //millisecond 
    };
    if (/(y+)/.test(format))
        format = format.replace(RegExp.$1,
                (this.getFullYear() + "").substr(4 - RegExp.$1.length));
    for (var k in o)
        if (new RegExp("(" + k + ")").test(format))
            format = format.replace(RegExp.$1,
                    RegExp.$1.length == 1 ? o[k] :
                        ("00" + o[k]).substr(("" + o[k]).length));
    return format;
};
function round(v, e) {
    var t = 1;
    for (; e > 0; t *= 10, e--);
    for (; e < 0; t /= 10, e++);
    return Math.round(v * t) / t;
}
//状态
function state(value) {
    if (value == 0 || value == "0") {
        return "<font color=#d02e2e>未审核</font>"
    }
    else if (value == 1 || value == "1") {
        return "<font color=#1fc73a>已同意</font>"
    }
    else if (value == 2 || value == "2") {
        return "<font color=#d02e2e>不同意</font>"
    }
    else {
        return "<font color=#d02e2e>未审核</font>"
    }
    }
function stateSecond(value) {
    if (value == 0 || value == "0") {
        return "<font color=#d02e2e>审核中</font>"
    }
    else if (value == 1 || value == "1") {
        return "<font color=#1fc73a>已同意</font>"
    }
    else if (value == 2 || value == "2") {
        return "<font color=#d02e2e>不同意</font>"
    }
    else if (value == 3 || value == "3") {
        return "<font color=#d02e2e>已作废</font>"
    }
    else {
        return "<font color=#d02e2e>未审核</font>"
    }
    }
function definedState(check, agree, disagree, value) {
    if (value == check) {
        return "<font color=#d02e2e>未审核</font>"
    }
    else if (value == agree) {
        return "<font color=#1fc73a>已同意</font>"
    }
    else if (value == disagree) {
        return "<font color=#d02e2e>不同意</font>"
    }
    else {
        return "<font color=#2967c9>审核中</font>"
    }
}