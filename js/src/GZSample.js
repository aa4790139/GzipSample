//*************************************************************************
//     创建日期:     2020-12-16 04:12:30
//     文件名称:     GZSample.js
//     创建作者:     Harry
//     版权所有:     剑齿虎
//     开发版本:     V1.0
//     相关说明:     JS 与 CSharp :  GZip 之间的压缩与解压Demo
//*************************************************************************

var fs = require("fs");
var path = require("path");
var gzip = require("./lib/gzip.js");

//-------------------------------------------------------------------------
/**
 * 二进制数组转String
 * @param array
 * @returns {string}
 */
function bin2String(array) {
    var result = "";
    for (var i = 0; i < array.length; i++) {
        result += String.fromCharCode(parseInt(array[i]));
    }
    return result;
}

//-------------------------------------------------------------------------
/**
 * String 转二进制数组
 * @param str
 * @returns {[]}
 */
function string2Bin(str) {
    var result = [];
    for (var i = 0; i < str.length; i++) {
        result.push(str.charCodeAt(i));
    }
    return result;
}

//-------------------------------------------------------------------------
/**
 * JS 压缩 Gzip demo
 */
function compressGZIPDemo() {
    console.log("compressGZIPDemo: ");
    var gzBytes = gzip.zip('java script', {
        timestamp: parseInt(Date.now() / 1000, 10)
    });
    var outpath = path.join(__dirname, "../res/gz/js.gz");
    fs.writeFileSync(outpath, new Buffer(gzBytes));
    console.log("gzContent=" + bin2String(gzBytes));
    //-------------------------------------------------------------------------
    var fileBytes = fs.readFileSync(outpath);
    var unzipBytes = gzip.unzip(fileBytes, {});
    var unzipContent = bin2String(unzipBytes);
    console.log("ungzContent:" + unzipContent);

    console.log("========================================");
}

compressGZIPDemo();

//-------------------------------------------------------------------------
/**
 * JS 压缩 Gzip + base64 demo
 */
function compressGZIPBase64Demo() {
    console.log("compressGZIPBase64Demo: ");
    var gzBytes = gzip.zip('java script base64', {
        timestamp: parseInt(Date.now() / 1000, 10)
    });

    let buff = new Buffer(gzBytes);
    let base64data = buff.toString('base64');

    var outpath = path.join(__dirname, "../res/gz/js_base64.gz");
    fs.writeFileSync(outpath, base64data);
    console.log("gzBase64Content=" + base64data);
    //-------------------------------------------------------------------------
    var fileContent = fs.readFileSync(outpath, {encoding: "utf-8"});
    var decodeBase64Buffer = new Buffer(fileContent, 'base64');
    var decodeBase64Bytes = Array.prototype.slice.call(buff, 0, buff.length);
    var unzipBytes = gzip.unzip(decodeBase64Bytes, {});
    var unzipContent = bin2String(unzipBytes);
    console.log("ungzBase64Content:" + unzipContent);
    console.log("========================================");
}

compressGZIPBase64Demo();

//-------------------------------------------------------------------------

/**
 * JS 解压 CSharp 的 Gzip demo
 */
function deCompressGZIPDemo() {
    console.log("deCompressGZIPDemo: ");
    var filepath = "../res/gz/csharp.gz";
    var fileBytes = fs.readFileSync(path.join(__dirname, filepath));
    console.log("gzContent:" + bin2String(fileBytes));

    var outpath = path.join(__dirname, "../res/ungz/csharp_gz_content.txt");
    var output = gzip.unzip(fileBytes, {});

    console.log("ungzContent:" + bin2String(output));

    fs.writeFileSync(outpath, new Buffer(output));
    console.log("========================================");
}

deCompressGZIPDemo();

//-------------------------------------------------------------------------
/**
 * JS 解压 CSharp 的 base64 + gzip
 */
function deCompressGZIPBase64Demo() {
    console.log("deCompressGZIPBase64Demo: ");
    var filePath = "../res/gz/csharp_base64.gz";
    var fileContent = fs.readFileSync(path.join(__dirname, filePath), {encoding: "utf-8"});
    console.log("gzBase64Content:" + fileContent);

    //decode base64
    let buff = new Buffer(fileContent, 'base64');

    // 解压gzip
    var decodedBytes = Array.prototype.slice.call(buff, 0, buff.length);
    var output = gzip.unzip(decodedBytes, {});
    console.log("ungzBase64Content:" + bin2String(output));

    var outPath = path.join(__dirname, "../res/ungz/csharp_base64_gz_content.txt");
    fs.writeFileSync(outPath, new Buffer(output));
    console.log("========================================");
}

deCompressGZIPBase64Demo();
//-------------------------------------------------------------------------




