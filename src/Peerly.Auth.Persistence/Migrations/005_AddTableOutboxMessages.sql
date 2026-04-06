-- +goose Up
-- +goose StatementBegin
create table outbox_messages
(
    id             bigserial   primary key not null,
    event_type     text                    not null,
    topic          text                    not null,
    key            text                    not null,
    payload        text                    not null,
    creation_time  timestamptz             not null,
    processed_time timestamptz,
    fail_count     integer                 not null,
    error          text
);

create index idx_outbox_messages_unprocessed
    on outbox_messages (topic, id)
    where processed_time is null;
-- +goose StatementEnd


-- +goose Down
-- +goose StatementBegin
drop table outbox_messages;
-- +goose StatementEnd
