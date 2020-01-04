
function DingLogin(elementID, appid, redirect_uri, width, height)
{
	var url = "https://oapi.dingtalk.com/connect/oauth2/sns_authorize?appid=" + appid + "&response_type=code&scope=snsapi_login&state=STATE&redirect_uri=" + redirect_uri;
	var obj = DDLogin({
		id: elementID,
		goto: encodeURIComponent(url),
		style: "border:none;background-color:rgba(128,155,225,0);margin:0,auto;",
		width: width,
		height: height
	});
	//fde668
	var hanndleMessage = function (event) {

		var origin = event.origin;
		console.log("origin", event.origin);
		if (origin == "https://login.dingtalk.com") { //判断是否来自ddLogin扫码事件。
			var loginTmpCode = event.data; //拿到loginTmpCode后就可以在这里构造跳转链接进行跳转了
			console.log(loginTmpCode);

			var rr = "http://120.24.176.207:36/Home/Log"; //公网
			var urlr = "https://oapi.dingtalk.com/connect/oauth2/sns_authorize?appid=" + appid + "&response_type=code&scope=snsapi_login&state=STATE&redirect_uri=" + redirect_uri + "&loginTmpCode=" + loginTmpCode;

			// var rr = "  http://localhost:53007/Home/Log"; //本地
			//var urlr = "https://oapi.dingtalk.com/connect/oauth2/sns_authorize?appid=dingoaxdcsdydsvkggrmfr&response_type=code&scope=snsapi_login&state=STATE&redirect_uri=" + rr + "&loginTmpCode=" + loginTmpCode;
			window.location.href = urlr;
		}

	};
	if (typeof window.addEventListener != 'undefined') {
		window.addEventListener('message', hanndleMessage, false);
	}
	else if (typeof window.attachEvent != 'undefined') {
		window.attachEvent('onmessage', hanndleMessage);
	}
}