CREATE TABLE IF NOT EXISTS users (
    user_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS games (
    game_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    title VARCHAR(150) NOT NULL,
    description TEXT,
    price NUMERIC(10, 2) NOT NULL DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS genres (
    genre_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS game_genres (
    game_id BIGINT REFERENCES games(game_id) ON DELETE CASCADE,
    genre_id BIGINT REFERENCES genres(genre_id) ON DELETE CASCADE,
    PRIMARY KEY (game_id, genre_id)
);

CREATE TABLE IF NOT EXISTS orders (
    order_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id BIGINT REFERENCES users(user_id),
    total NUMERIC(10, 2) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS order_items (
    order_item_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    order_id BIGINT REFERENCES orders(order_id) ON DELETE CASCADE,
    game_id BIGINT REFERENCES games(game_id),
    price NUMERIC(10, 2) NOT NULL
);

CREATE TABLE IF NOT EXISTS user_library (
    user_id BIGINT REFERENCES users(user_id) ON DELETE CASCADE,
    game_id BIGINT REFERENCES games(game_id) ON DELETE CASCADE,
    purchased_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (user_id, game_id)
);

CREATE TABLE IF NOT EXISTS reviews (
    review_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id BIGINT REFERENCES users(user_id) ON DELETE CASCADE,
    game_id BIGINT REFERENCES games(game_id) ON DELETE CASCADE,
    rating INTEGER NOT NULL CHECK (rating BETWEEN 1 AND 5),
    comment TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (user_id, game_id)
);

-- ЖАНРЫ
INSERT INTO genres (name) VALUES
('Экшен'),
('Приключения'),
('RPG'),
('Стратегия'),
('Гонки'),
('Хоррор')
ON CONFLICT (name) DO NOTHING;

-- ТЕСТОВЫЕ ИГРЫ
INSERT INTO games (title, description, price) VALUES
('Cyberpunk 2077', 'Футуристическая ролевая игра в открытом мире.', 1999),
('The Witcher 3', 'Большое приключение в мире монстров и магии.', 1499),
('Minecraft', 'Исследуй мир и создавай собственные постройки.', 999),
('Red Dead Redemption 2', 'Приключение на Диком Западе.', 2499),
('Forza Horizon 5', 'Гоночная игра с открытым миром.', 1799),
('DOOM Eternal', 'Динамичный фантастический экшен.', 1299)
ON CONFLICT DO NOTHING;

-- ПРИВЯЗКА ЖАНРОВ
INSERT INTO game_genres (game_id, genre_id)
SELECT g.game_id, gen.genre_id
FROM games g
CROSS JOIN genres gen
WHERE
    (g.title = 'Cyberpunk 2077' AND gen.name IN ('RPG', 'Экшен'))
 OR (g.title = 'The Witcher 3' AND gen.name IN ('RPG', 'Приключения'))
 OR (g.title = 'Minecraft' AND gen.name IN ('Приключения'))
 OR (g.title = 'Red Dead Redemption 2' AND gen.name IN ('Экшен', 'Приключения'))
 OR (g.title = 'Forza Horizon 5' AND gen.name IN ('Гонки'))
 OR (g.title = 'DOOM Eternal' AND gen.name IN ('Экшен'))
ON CONFLICT DO NOTHING;
