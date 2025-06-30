FROM php:7.4-cli as builder

RUN apt-get update && \
  apt-get install -y \
  git \
  unzip \
  libmagickwand-dev \
  libonig-dev \
  pkg-config

RUN docker-php-ext-install mbstring && \
  pecl install imagick && \
  docker-php-ext-enable imagick mbstring

RUN php -r "copy('https://getcomposer.org/installer', 'composer-setup.php');" && \
  php composer-setup.php --install-dir=/usr/local/bin --filename=composer && \
  rm composer-setup.php

WORKDIR /src
COPY ./ ./

RUN composer install --no-dev --optimize-autoloader

FROM php:7.4-cli

RUN apt-get update && \
  apt-get install -y \
  cups \
  avahi-daemon \
  dbus \
  net-tools \
  socat \
  tini \
  libmagickwand-dev \
  libonig5 && \
  pecl install imagick && \
  docker-php-ext-enable imagick && \
  apt-get clean && rm -rf /var/lib/apt/lists/*

COPY --from=builder /usr/local/lib/php/extensions/ /usr/local/lib/php/extensions/
RUN echo "extension=mbstring.so" > /usr/local/etc/php/conf.d/mbstring.ini

WORKDIR /app
COPY --from=builder /src /app

EXPOSE 8631

ENTRYPOINT ["/usr/bin/tini", "--"]

CMD ["/app/entrypoint.sh"]

# FROM ubuntu:22.04

# RUN apt-get update && apt-get install -y \
#   cups \
#   cups-ipp-utils \
#   cups-filters \
#   avahi-daemon \
#   dbus \
#   net-tools \
#   iptables \
#   socat \
#   tini \
#   && rm -rf /var/lib/apt/lists/*

# RUN mkdir -p /app/jobs /app/bin

# COPY entrypoint.sh /app/entrypoint.sh

# EXPOSE 8631

# ENTRYPOINT ["/usr/bin/tini", "--"]

# CMD ["/app/entrypoint.sh"]