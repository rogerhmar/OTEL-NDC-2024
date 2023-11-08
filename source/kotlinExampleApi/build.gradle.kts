val ktor_version: String by project
val kotlin_version: String by project
val logback_version: String by project
val otel_version = "1.31.0"

plugins {
    kotlin("jvm") version "1.9.20"
    id("io.ktor.plugin") version "2.3.5"
}

group = "com.fortedigital.observable"
version = "0.0.1"

application {
    mainClass.set("com.fortedigital.observable.ApplicationKt")

    val isDevelopment: Boolean = project.ext.has("development")
    applicationDefaultJvmArgs = listOf("-Dio.ktor.development=$isDevelopment")
}

repositories {
    mavenCentral()
}

dependencies {
    implementation("io.ktor:ktor-server-core-jvm")
    implementation("io.ktor:ktor-server-openapi")
    implementation("io.ktor:ktor-server-host-common-jvm")
    implementation("io.ktor:ktor-server-status-pages-jvm")
    implementation("io.ktor:ktor-server-swagger-jvm")
    implementation("io.ktor:ktor-server-netty-jvm")

    implementation("ch.qos.logback:logback-classic:$logback_version")

    implementation(platform("io.opentelemetry:opentelemetry-bom:$otel_version"))
    implementation("io.opentelemetry:opentelemetry-api")

    implementation("io.opentelemetry:opentelemetry-sdk:$otel_version");
    implementation("io.opentelemetry:opentelemetry-sdk-metrics:$otel_version");
    implementation("io.opentelemetry:opentelemetry-sdk-trace:$otel_version");
    implementation("io.opentelemetry:opentelemetry-sdk-extension-autoconfigure-spi:$otel_version");
    implementation("io.opentelemetry:opentelemetry-sdk-extension-autoconfigure:$otel_version");

    implementation("io.opentelemetry:opentelemetry-exporter-otlp:$otel_version");

    implementation("io.opentelemetry:opentelemetry-extension-kotlin:$otel_version");
    implementation("io.opentelemetry.instrumentation:opentelemetry-ktor-2.0:1.31.0-alpha");

    testImplementation("io.ktor:ktor-server-tests-jvm")
    testImplementation("org.jetbrains.kotlin:kotlin-test-junit:$kotlin_version")
}
