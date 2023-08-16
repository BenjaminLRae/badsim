L = 6.5; % L = aerodynamic length
Angle = 30; % launch angle

Ui = 100; % Initial Velocity
Ut = 6.8; % terminal velocity
U = Ui/Ut;

intlog = 1 + 4*(U^2) * sin(deg2rad(Angle));
X = ((L * cos(deg2rad(Angle)))/2) * log(intlog)