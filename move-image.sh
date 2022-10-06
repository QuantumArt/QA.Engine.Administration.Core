#!/bin/sh
docker pull '||registry-source||widget-admin:||version||' && \
docker tag '||registry-source||widget-admin:||version||' '||registry-receiver||widget-admin:||version||' && \
docker push '||registry-receiver||widget-admin:||version||'