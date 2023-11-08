package com.fortedigital.observable

import com.fortedigital.observable.plugins.configureHTTP
import com.fortedigital.observable.plugins.configureOtel
import com.fortedigital.observable.plugins.configureRouting
import io.ktor.server.application.*
import io.ktor.server.engine.*
import io.ktor.server.netty.*



fun main() {
    embeddedServer(Netty, port = 5010, host = "0.0.0.0", module = Application::module)
            .start(wait = true)
}

fun Application.module() {
    configureOtel()
    configureHTTP()
    configureRouting()
}
