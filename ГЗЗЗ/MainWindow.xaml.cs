using System.Windows;

namespace ГЗЗЗ
{
    /// <summary>
    /// Логика взаимодействия для главного окна приложения (панель управления).
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Авторизованный пользователь, вошедший в систему.
        /// </summary>
        private Users currentUser;

        /// <summary>
        /// Инициализирует главное окно. 
        /// Приветствует пользователя и скрывает элементы управления для не-администраторов.
        /// </summary>
        /// <param name="user">Объект авторизованного пользователя.</param>
        public MainWindow(Users user)
        {
            InitializeComponent();
            currentUser = user;
            TxtWelcome.Text = $"Добро пожаловать, {user.Login} ({user.Role})";
            string role = user.Role.Trim().ToLower();

            // Проверка роли: если это админ, загружаем список юзеров, иначе скрываем панель
            if (role.Contains("админ") || role.Contains("admin"))
            {
                LoadUsers();
            }
            else
            {
                AdminPanel.Visibility = Visibility.Collapsed;
                GridUsers.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Загружает список всех пользователей из БД в таблицу (DataGrid).
        /// </summary>
        private void LoadUsers()
        {
            GridUsers.ItemsSource = Db.GetAllUsers();
        }

        /// <summary>
        /// Обработчик кнопки "Добавить". Открывает окно создания нового пользователя.
        /// </summary>
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (new UserEditWindow(null).ShowDialog() == true) LoadUsers();
        }

        /// <summary>
        /// Обработчик кнопки "Редактировать". Открывает окно для выбранного пользователя.
        /// </summary>
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (GridUsers.SelectedItem is Users u && new UserEditWindow(u).ShowDialog() == true) LoadUsers();
        }

        /// <summary>
        /// Обработчик кнопки "Разблокировать". Снимает блокировку с выбранного пользователя.
        /// </summary>
        private void BtnUnblock_Click(object sender, RoutedEventArgs e)
        {
            if (GridUsers.SelectedItem is Users u)
            {
                Db.Unblock(u.Login);
                LoadUsers();
            }
        }

        /// <summary>
        /// Обработчик кнопки "Выйти". Возвращает на окно авторизации и закрывает текущее.
        /// </summary>
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            Close();
        }
    }
}