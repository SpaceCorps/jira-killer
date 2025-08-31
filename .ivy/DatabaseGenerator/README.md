# JiraKiller 

## Schema

```dbml 
Enum issue_type {
    bug
    task
    story
    epic
}

Enum priority {
    lowest
    low
    medium
    high
    highest
}

Enum status {
    open
    in_progress
    resolved
    closed
    reopened
}

Enum role {
    admin
    developer
    tester
    viewer
}

Table users {
    id int [pk, increment, not null]
    username varchar [not null, unique]
    email varchar [not null, unique]
    full_name varchar [not null]
    created_at timestamp [not null]
    updated_at timestamp [not null]
}

Table projects {
    id int [pk, increment, not null]
    name varchar [not null]
    project_key varchar [not null, unique]
    description text [null]
    created_at timestamp [not null]
    updated_at timestamp [not null]
}

Table project_user {
    project_id int [not null]
    user_id int [not null]
    role role [not null]
    indexes {
        (project_id, user_id) [pk]
    }
}

Table issues {
    id int [pk, increment, not null]
    project_id int [not null]
    reporter_id int [not null]
    assignee_id int [null]
    issue_type issue_type [not null]
    priority priority [not null]
    status status [not null]
    summary varchar [not null]
    description text [null]
    created_at timestamp [not null]
    updated_at timestamp [not null]
}

Table comments {
    id int [pk, increment, not null]
    issue_id int [not null]
    author_id int [not null]
    body text [not null]
    created_at timestamp [not null]
    updated_at timestamp [not null]
}

Table worklogs {
    id int [pk, increment, not null]
    issue_id int [not null]
    user_id int [not null]
    time_spent_minutes int [not null]
    started_at timestamp [not null]
    comment text [null]
    created_at timestamp [not null]
    updated_at timestamp [not null]
}

Table attachments {
    id int [pk, increment, not null]
    issue_id int [not null]
    uploader_id int [not null]
    filename varchar [not null]
    file_path varchar [not null]
    file_size int [not null]
    created_at timestamp [not null]
    updated_at timestamp [not null]
}

Ref: projects.id > issues.project_id
Ref: users.id > issues.reporter_id
Ref: users.id > issues.assignee_id
Ref: users.id > comments.author_id
Ref: issues.id > comments.issue_id
Ref: users.id > worklogs.user_id
Ref: issues.id > worklogs.issue_id
Ref: users.id > attachments.uploader_id
Ref: issues.id > attachments.issue_id
Ref: projects.id > project_user.project_id
Ref: users.id > project_user.user_id
```