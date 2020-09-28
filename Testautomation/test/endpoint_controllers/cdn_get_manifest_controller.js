const axios = require("axios");
const AdmZip = require("adm-zip");
const axiosLogger = require("axios-logger");
const fs = require('fs');

async function manifest(endpoint) {

    const instance = axios.create();
    // add start time header in request
    instance.interceptors.request.use((config) => {
        config.headers["request-startTime"] = process.hrtime();
        return config;
    });

    // add duration header into response
    instance.interceptors.response.use((response) => {
        const start = response.config.headers["request-startTime"];
        const end = process.hrtime(start);
        const milliseconds = Math.round(end[0] * 1000 + end[1] / 1000000);
        response.headers["request-duration"] = milliseconds;
        return response;
    });

    // add logging on request and response
    instance.interceptors.request.use(axiosLogger.requestLogger,axiosLogger.errorLogger);
    instance.interceptors.response.use(
        res => {
            console.log('[Axios][Response] ' + res.config.method, res.config.url,res.status);
            return res;
        },  axiosLogger.errorLogger);

    const response = await instance({
        method: "get",
        url:
            endpoint,
        headers: { Accept: "application/json" },
        responseType: "arraybuffer",
        responseEncoding: null,
    })

    let zip = new AdmZip(response.data);
    fs.writeFileSync( `util/temp/manifest_response.zip`, zip);

    let zipEntries = zip.getEntries();
    content = JSON.parse(zip.readAsText(zipEntries[0]).toString('utf8'));

    let obj = {"content":content,
               "response": response,
    };
    return obj;
};

module.exports = manifest;
