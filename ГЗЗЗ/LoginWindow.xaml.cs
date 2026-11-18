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
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private int[] order = { 0, 1, 2, 3 };
        private Button[] tiles = new Button[4];
        private ImageSource[] pieces = new ImageSource[4];
        private int? selected;

        /// <summary>
        /// Конструктор окна. Инициализирует компоненты интерфейса, создает и перемешивает пазл.
        /// </summary>
        public LoginWindow()
        {
            InitializeComponent();
            CreatePuzzle();
            Shuffle();
        }

        /// <summary>
        /// Загружает четыре изображения из ресурсов проекта и создает интерактивные кнопки 
        /// для отображения фрагментов пазла в сетке.
        /// </summary>
        private void CreatePuzzle()
        {
            pieces[0] = new BitmapImage(new Uri("pack://application:,,,/Images/1.png"));
            pieces[1] = new BitmapImage(new Uri("pack://application:,,,/Images/2.png"));
            pieces[2] = new BitmapImage(new Uri("pack://application:,,,/Images/3.png"));
            pieces[3] = new BitmapImage(new Uri("pack://application:,,,/Images/4.png"));

            for (int i = 0; i < 4; i++)
            {
                int pos = i;
                tiles[i] = new Button { BorderThickness = new Thickness(1), BorderBrush = Brushes.Gray };
                tiles[i].Click += (s, e) => TileClick(pos);
                PuzzleGrid.Children.Add(tiles[i]);
            }
        }

        /// <summary>
        /// Случайным образом перемешивает порядок фрагментов пазла. 
        /// Гарантирует, что изначально пазл не будет собран в правильном порядке.
        /// </summary>
        private void Shuffle()
        {
            var rnd = new Random();
            do { order = order.OrderBy(x => rnd.Next()).ToArray(); }
            while (order.SequenceEqual(new[] { 0, 1, 2, 3 }));

            selected = null;
            DrawTiles();
        }

        /// <summary>
        /// Обновляет визуальное отображение фрагментов пазла на кнопках. 
        /// Выделяет выбранный пользователем фрагмент красной рамкой.
        /// </summary>
        private void DrawTiles()
        {
            for (int i = 0; i < 4; i++)
            {
                tiles[i].Background = new ImageBrush(pieces[order[i]]) { Stretch = Stretch.UniformToFill };
                tiles[i].BorderBrush = (selected == i) ? Brushes.Red : Brushes.Gray;
                tiles[i].BorderThickness = (selected == i) ? new Thickness(3) : new Thickness(1);
            }
        }

        /// <summary>
        /// Обрабатывает клик по фрагменту пазла. 
        /// Запоминает первый выбранный фрагмент и меняет его местами со вторым при повторном клике.
        /// </summary>
        /// <param name="pos">Индекс (позиция) нажатого фрагмента.</param>
        private void TileClick(int pos)
        {
            if (selected == null) { selected = pos; DrawTiles(); return; }
            if (selected == pos) { selected = null; DrawTiles(); return; }
            int tmp = order[selected.Value];
            order[selected.Value] = order[pos];
            order[pos] = tmp;

            selected = null;
            DrawTiles();
        }

        /// <summary>
        /// Обработчик кнопки "Перемешать". Принудительно перемешивает пазл заново.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные события.</param>
        private void BtnShuffle_Click(object sender, RoutedEventArgs e) => Shuffle();

        /// <summary>
        /// Обрабатывает нажатие кнопки "Войти". Проверяет заполненность полей, 
        /// статус блокировки пользователя, правильность сборки пазла и корректность пароля. 
        /// Выводит соответствующие сообщения и открывает главное окно при успехе.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные события.</param>
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = TxtLogin.Text.Trim();
            string pass = TxtPass.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var user = Db.GetUser(login);
            if (user != null && user.IsBlocked)
            {
                MessageBox.Show("Вы заблокированы. Обратитесь к администратору.", "Блокировка", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
            bool isSolved = order.SequenceEqual(new[] { 0, 1, 2, 3 });

            if (!isSolved)
            {
                Db.RegisterFail(login);
                MessageBox.Show("Пазл собран неверно!", "Ошибка капчи", MessageBoxButton.OK, MessageBoxImage.Warning);
                Shuffle();
                return;
            }

            if (user == null || user.Password != pass)
            {
                Db.RegisterFail(login);
                MessageBox.Show("Вы ввели неверный логин или пароль. Пожалуйста проверьте ещё раз введенные данные.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Shuffle();
                return;
            }

            Db.ResetFail(login);
            MessageBox.Show("Вы успешно авторизовались", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            new MainWindow(user).Show();
            Close();
        }
    }
}