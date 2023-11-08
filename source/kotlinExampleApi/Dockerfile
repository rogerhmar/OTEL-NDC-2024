FROM gradle:7.1.0-jdk11 as build

COPY --chown=gradle:gradle . /home/gradle/src
WORKDIR /home/gradle/src
RUN gradle build --no-daemon

FROM openjdk:22-slim
RUN mkdir /app
COPY --from=build /home/gradle/src/build/libs/ /app/
ENTRYPOINT ["java","-jar","/app/ktor-example-api-all.jar"]