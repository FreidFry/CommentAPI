rabbitmqctl add_user "user" "password" || true
rabbitmqctl set_user_tags "user" administrator
rabbitmqctl set_permissions -p "/" "user" ".*" ".*" ".*"