package com.fortedigital.observable

import com.fortedigital.observable.plugins.*
import io.ktor.client.request.*
import io.ktor.client.statement.*
import io.ktor.http.*
import io.ktor.server.testing.*
import kotlin.test.*

class ApplicationTest {
    @Test
    fun testRoot() = testApplication {
        application {
            configureRouting()
        }
        client.get("/hello").apply {
            assertEquals(HttpStatusCode.OK, status)
            assertEquals("Hello Dotnet!", bodyAsText())
        }
    }
}
