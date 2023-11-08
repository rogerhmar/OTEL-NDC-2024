package com.fortedigital.observable.plugins

import io.ktor.http.*
import io.ktor.server.application.*
import io.ktor.server.http.content.*
import io.ktor.server.plugins.openapi.*
import io.ktor.server.plugins.statuspages.*
import io.ktor.server.response.*
import io.ktor.server.routing.*
import io.opentelemetry.api.GlobalOpenTelemetry
import io.opentelemetry.api.OpenTelemetry
import io.opentelemetry.api.trace.StatusCode
import io.opentelemetry.context.Context
import io.opentelemetry.extension.kotlin.asContextElement
import io.opentelemetry.sdk.OpenTelemetrySdk
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch
import java.io.File

fun Application.configureRouting() {
    install(StatusPages) {
        exception<Throwable> { call, cause ->
            call.respondText(text = "500: $cause", status = HttpStatusCode.InternalServerError)
        }
    }
    val tracer = GlobalOpenTelemetry.getTracer("KtorExampleAPI")
    routing {
        get("/hello") {
            tracer.spanBuilder("hello").startSpan().run {
                setAttribute("hello", "world")
                addEvent("handling this...")
                delay(100)
                call.respondText("Hello Dotnet!")
                end()
            }
        }
        get("/error") {
            tracer.spanBuilder("error").startSpan().run {
                setAttribute("error", "ugly error")
                addEvent("big nasty exception")
                this.setStatus(StatusCode.ERROR)
                delay(100)
                call.respondText("Hello World!")
                end()
            }
        }

        // Static plugin. Try to access `/static/index.html`
        staticFiles("/static", File("/static"), "index.html") {
            staticResources("static", "static")
        }
        openAPI(path = "openapi", swaggerFile = "openapi/documentation.yaml")

    }

}
