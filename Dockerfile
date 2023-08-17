FROM ubuntu:22.04
RUN apt-get update && \
    apt-get install -y ca-certificates && \
    update-ca-certificates \
    
WORKDIR /app
COPY ./build/LinuxServer ./Server
RUN chmod +x /app/Server

CMD /app/Server -batchmode -nographics
