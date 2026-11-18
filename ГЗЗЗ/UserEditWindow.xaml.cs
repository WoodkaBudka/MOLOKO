using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ГЗЗЗ
{
    /// <summary>
    /// Логика взаимодействия для окна создания/редактирования пользователя.
    /// </summary>
    public partial class UserEditWindow : Window
    {
        /// <summary>
        /// Текущий редактируемый пользователь. Если null, создается новый.
        /// </summary>
        private Users user;

        /// <summary>
        /// Инициализирует окно редактирования. 
        /// Заполняет поля данными пользователя или создает нового с ролью по умолчанию.
        /// </summary>
        /// <param name="u">Объект пользователя для редактирования (или null для создания нового).</param>
        public UserEditWindow(Users u)
        {
            InitializeComponent();
            user = u ?? new Users { Role = "Пользователь" };

            TxtLogin.Text = user.Login;
            TxtPass.Text = user.Password;
            CmbRole.Text = user.Role;
            ChkBlocked.IsChecked = user.IsBlocked;
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Сохранить". 
        /// Проверяет заполнение полей и уникальность логина, затем сохраняет данные в БД.
        /// </summary>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtLogin.Text) || string.IsNullOrWhiteSpace(TxtPass.Text))
            {
                MessageBox.Show("Заполните логин и пароль!");
                return;
            }

            if (Db.LoginExists(TxtLogin.Text, user.ID))
            {
                MessageBox.Show("Такой логин уже есть!");
                return;
            }

            user.Login = TxtLogin.Text;
            user.Password = TxtPass.Text;
            user.Role = CmbRole.Text;
            user.IsBlocked = ChkBlocked.IsChecked == true;

            Db.SaveUser(user);
            DialogResult = true;
        }
    }
}