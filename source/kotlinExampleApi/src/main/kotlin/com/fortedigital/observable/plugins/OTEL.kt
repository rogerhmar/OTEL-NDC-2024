package com.fortedigital.observable.plugins

import io.ktor.server.application.*
import io.opentelemetry.exporter.otlp.metrics.OtlpGrpcMetricExporter
import io.opentelemetry.exporter.otlp.trace.OtlpGrpcSpanExporter
import io.opentelemetry.sdk.OpenTelemetrySdk
import io.opentelemetry.sdk.metrics.SdkMeterProvider
import io.opentelemetry.sdk.metrics.export.PeriodicMetricReader
import io.opentelemetry.sdk.resources.Resource
import io.opentelemetry.sdk.trace.SdkTracerProvider
import io.opentelemetry.sdk.trace.export.BatchSpanProcessor
import io.opentelemetry.semconv.ResourceAttributes.SERVICE_NAME
import java.util.concurrent.TimeUnit

fun Application.configureOtel() {
    val collectorUrl = System.getenv("otelCollector") ?: "http://localhost:4317"
    println("Collector URL: $collectorUrl")

    val resource = Resource.getDefault()
        .merge(Resource.builder().put(SERVICE_NAME, "KtorExampleAPI").build())

    OpenTelemetrySdk.builder()
        .setTracerProvider(SdkTracerProvider
            .builder()
            .setResource(resource)
            .addSpanProcessor(
                BatchSpanProcessor.builder(
                    OtlpGrpcSpanExporter.builder()
                        .setEndpoint(collectorUrl)
                        .setTimeout(2, TimeUnit.SECONDS)
                        .build()
                )
                    .setScheduleDelay(1, TimeUnit.SECONDS)
                    .build())
            .build())
        .setMeterProvider(SdkMeterProvider
            .builder()
            .setResource(resource)
            .registerMetricReader(
                PeriodicMetricReader.builder(
                    OtlpGrpcMetricExporter.builder()
                        .setEndpoint(collectorUrl)
                        .setTimeout(500, TimeUnit.MILLISECONDS)
                        .build()
                )
                    .setInterval(1, TimeUnit.SECONDS)
                    .build())
            .build())
        .buildAndRegisterGlobal()
}
