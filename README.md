# Kafka Sagas Demo

## Overview

This was built as a POC to validate the usage of the Sagas pattern using some of the components provided by the Masstransit package. Here we tested some of the most basic Masstransit features related to the Sagas pattern implementation such as State Machines, State Machines Activities, Events Orchestration, Message Retry/Redelivery and Instance persistence to name a few. To develop this we used the [Masstransit documentation](https://masstransit.io/) and the [Kafka YouTube series](https://www.youtube.com/watch?v=CJ_srcJiIKs&list=PLx8uyNNs1ri0RJ3hqwcDze6yAkrmK1QI5 ) by Chris Patterson.

## Basic sagas flow

Here we are presenting a basic flow of how all the components should work. We also implemented a fault recovery pipeline using an error topic.

![alt text](/static/sagas-flow.png)

## External servers

To develop this application, we used some popular servers such as [Confluent Kafka Cloud](https://www.confluent.io/), [MongoDB Atlas](https://www.mongodb.com/atlas) and [Redis on Cloud](https://redis.com/). All the mentioned resources are very easy to use and have free trial plans.

## Usage

Once you have the codebase, simply adjust the configuration files with your configuration and run all the projects. To produce messages to the Kafka server, use the `OrderManagentApi`. To facilitate, we are providing the topics list below so it will be easier to replicate it on your server. As far as database configuration, MassTransit makes it very easy, requiring minimal manual configuration. So, assuming you have your database server up and running, everything should work out of the box for both MongoDB and Redis.

### Topics list
- ORDER_MANAGEMENT_SYSTEM.REQUEST
- ORDER_MANAGEMENT_SYSTEM.RESPONSE
- TAXES_CALCULATION_ENGINE.REQUEST
- TAXES_CALCULATION_ENGINE.RESPONSE
- CUSTOMER_VALIDATION_ENGINE.REQUEST
- CUSTOMER_VALIDATION_ENGINE.RESPONSE
- ERROR

## Important

You can run database and Kafka servers locally. Just make sure to adjust all the configurations within all the runnable projects.
If you're having a hard time trying to run this demo application, feel free to contact me and I will try to help the best way I can.