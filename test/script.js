import http from 'k6/http';
import { check } from 'k6';
import { htmlReport } from "https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js";

export function handleSummary(data) {
    return {
        "summary.html": htmlReport(data),
    };
}


export const options = {
    thresholds: {
        http_req_failed: ['rate<0.001'], // http errors should be less than 0.1%
        http_req_duration: ['p(95)<2000'], // 95% of requests should be below 2000ms
    },
    vus: 5000, // Virtual users - Parallel threads
    duration: '10s',
    insecureSkipTLSVerify: false, // Problems with the self signed certificate!!
};

export default function () {
    const res = http.get('http://localhost:5099/serial');
    check(res, {
        'is status 200': (r) => r.status === 200,
    });
}