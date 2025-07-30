import javax.swing.*;
import java.awt.*;
import java.io.File;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Paths;

public class Core {
    private static final String LOG_FILE = Paths.get(
            System.getProperty("user.home"),
            "Documents",
            "statux-data",
            "statuxCoreLog.txt"
    ).toString();

    public static void main(String[] args) {
        // Create the main frame
        JFrame frame = new JFrame("Statux Log Viewer");
        frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
        frame.setSize(800, 600);
        frame.setLayout(new BorderLayout());

        // Create components
        JTextArea textArea = new JTextArea();
        textArea.setEditable(false);
        JScrollPane scrollPane = new JScrollPane(textArea);
        JButton refreshButton = new JButton("Refresh Log");

        // Add action to button
        refreshButton.addActionListener(e -> {
            String logContent = readLogFile();
            textArea.setText(logContent);
        });

        // Add components to frame
        frame.add(scrollPane, BorderLayout.CENTER);
        frame.add(refreshButton, BorderLayout.SOUTH);

        // Initial load
        String logContent = readLogFile();
        textArea.setText(logContent);

        // Display the frame
        frame.setVisible(true);
    }

    private static String readLogFile() {
        try {
            if (new File(LOG_FILE).exists()) {
                return new String(Files.readAllBytes(Paths.get(LOG_FILE)));
            } else {
                return "Log file not found at:\n" + LOG_FILE;
            }
        } catch (IOException e) {
            return "Error reading log file:\n" + e.getMessage();
        }
    }
}