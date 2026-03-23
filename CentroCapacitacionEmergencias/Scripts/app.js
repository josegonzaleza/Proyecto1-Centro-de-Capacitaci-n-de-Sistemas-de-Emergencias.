function mostrarAlerta(mensaje) {
    alert(mensaje);
}

function obtenerValor(id) {
    var elemento = document.getElementById(id);
    return elemento ? elemento.value : '';
}

function setHtml(id, html) {
    var elemento = document.getElementById(id);
    if (elemento) {
        elemento.innerHTML = html;
    }
}

function setTexto(id, texto) {
    var elemento = document.getElementById(id);
    if (elemento) {
        elemento.innerText = texto;
    }
}

function postFormUrlEncoded(url, body) {
    return fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: body
    });
}

function postJson(url, data) {
    return fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json; charset=UTF-8',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: JSON.stringify(data)
    });
}

// =============================
// PARTICIPANTES
// =============================

function validarFormularioParticipante() {
    var tipo = obtenerValor('TipoIdentificacion');
    var identificacion = obtenerValor('Identificacion');
    var nombre = obtenerValor('NombreCompleto');
    var fechaNacimiento = obtenerValor('FechaNacimiento');
    var provincia = obtenerValor('Provincia');
    var canton = obtenerValor('Canton');
    var distrito = obtenerValor('Distrito');
    var detalleDireccion = obtenerValor('DetalleDireccion');
    var estadoCivil = obtenerValor('EstadoCivil');
    var correo = obtenerValor('Correo');

    if (!tipo || !identificacion || !nombre || !fechaNacimiento || !provincia || !canton || !distrito || !detalleDireccion || !estadoCivil || !correo) {
        mostrarAlerta('Debe completar todos los campos obligatorios del participante.');
        return false;
    }

    return true;
}

function buscarParticipantesAjax() {
    var texto = obtenerValor('filtroTexto');
    var cohorteId = obtenerValor('filtroCohorte');
    var cursoId = obtenerValor('filtroCurso');

    var url = '/Participantes/Buscar?texto=' + encodeURIComponent(texto)
        + '&cohorteId=' + encodeURIComponent(cohorteId)
        + '&cursoId=' + encodeURIComponent(cursoId);

    console.log('Buscar participantes:', url);

    fetch(url, {
        method: 'GET',
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(function (response) {
            if (!response.ok) {
                throw new Error('HTTP ' + response.status);
            }
            return response.text();
        })
        .then(function (html) {
            setHtml('contenedorParticipantes', html);
        })
        .catch(function (error) {
            console.error('Error buscarParticipantesAjax:', error);
            mostrarAlerta('Ocurrió un error al buscar participantes.');
        });
}

function desactivarParticipanteAjax(id) {
    if (!confirm('¿Desea desactivar este participante?')) return;

    console.log('Desactivar participante:', id);

    postFormUrlEncoded('/Participantes/DesactivarAjax', 'id=' + encodeURIComponent(id))
        .then(function (response) {
            if (!response.ok) throw new Error('HTTP ' + response.status);
            return response.json();
        })
        .then(function (data) {
            console.log('Respuesta desactivar:', data);
            mostrarAlerta(data.mensaje);
            if (data.ok) {
                buscarParticipantesAjax();
            }
        })
        .catch(function (error) {
            console.error('Error desactivarParticipanteAjax:', error);
            mostrarAlerta('Ocurrió un error al desactivar el participante.');
        });
}

function asignarParticipanteAjax() {
    var participanteId = obtenerValor('ParticipanteIdAsignacion');
    var cohorteId = obtenerValor('CohorteIdAsignacion');
    var cursosSelect = document.getElementById('CursosIdsAsignacion');
    var cursosIds = [];

    if (!participanteId || !cohorteId) {
        mostrarAlerta('Debe seleccionar una cohorte.');
        return;
    }

    if (!cursosSelect) {
        mostrarAlerta('No se encontró la lista de cursos.');
        return;
    }

    for (var i = 0; i < cursosSelect.options.length; i++) {
        if (cursosSelect.options[i].selected) {
            cursosIds.push(parseInt(cursosSelect.options[i].value, 10));
        }
    }

    if (cursosIds.length === 0) {
        mostrarAlerta('Debe seleccionar al menos un curso.');
        return;
    }

    console.log('Asignar participante:', {
        ParticipanteId: parseInt(participanteId, 10),
        CohorteId: parseInt(cohorteId, 10),
        CursosIds: cursosIds
    });

    postJson('/Participantes/AsignarAjax', {
        ParticipanteId: parseInt(participanteId, 10),
        CohorteId: parseInt(cohorteId, 10),
        CursosIds: cursosIds
    })
        .then(function (response) {
            if (!response.ok) throw new Error('HTTP ' + response.status);
            return response.json();
        })
        .then(function (data) {
            console.log('Respuesta asignar:', data);
            mostrarAlerta(data.mensaje);
        })
        .catch(function (error) {
            console.error('Error asignarParticipanteAjax:', error);
            mostrarAlerta('Ocurrió un error al asignar cohorte y cursos.');
        });
}

function removerCursoAjax(participanteId, cursoId) {
    var motivo = prompt('Ingrese el motivo del cambio o remoción del curso:', '') || '';

    console.log('Remover curso:', participanteId, cursoId, motivo);

    postFormUrlEncoded(
        '/Participantes/RemoverCursoAjax',
        'participanteId=' + encodeURIComponent(participanteId) +
        '&cursoId=' + encodeURIComponent(cursoId) +
        '&motivoCambio=' + encodeURIComponent(motivo)
    )
        .then(function (response) {
            if (!response.ok) throw new Error('HTTP ' + response.status);
            return response.json();
        })
        .then(function (data) {
            console.log('Respuesta remover curso:', data);
            mostrarAlerta(data.mensaje);
            if (data.ok) {
                location.reload();
            }
        })
        .catch(function (error) {
            console.error('Error removerCursoAjax:', error);
            mostrarAlerta('Ocurrió un error al remover el curso.');
        });
}

function cambiarCohorteAjax() {
    var participanteId = obtenerValor('ParticipanteIdCambioCohorte');
    var cohorteId = obtenerValor('NuevaCohorteId');
    var motivo = prompt('Ingrese el motivo del cambio de cohorte:', '') || '';

    if (!cohorteId) {
        mostrarAlerta('Debe seleccionar una cohorte.');
        return;
    }

    console.log('Cambiar cohorte:', participanteId, cohorteId, motivo);

    postFormUrlEncoded(
        '/Participantes/CambiarCohorteAjax',
        'participanteId=' + encodeURIComponent(participanteId) +
        '&cohorteId=' + encodeURIComponent(cohorteId) +
        '&motivoCambio=' + encodeURIComponent(motivo)
    )
        .then(function (response) {
            if (!response.ok) throw new Error('HTTP ' + response.status);
            return response.json();
        })
        .then(function (data) {
            console.log('Respuesta cambiar cohorte:', data);
            mostrarAlerta(data.mensaje);
            if (data.ok) {
                location.reload();
            }
        })
        .catch(function (error) {
            console.error('Error cambiarCohorteAjax:', error);
            mostrarAlerta('Ocurrió un error al cambiar la cohorte.');
        });
}

// =============================
// EVALUACIONES
// =============================

function validarFormularioEvaluacion() {
    var cursoId = obtenerValor('CursoIdEvaluacion');
    var destrezaId = obtenerValor('DestrezaIdEvaluacion');
    var minutos = obtenerValor('TiempoMinutos');
    var segundos = obtenerValor('TiempoSegundos');
    var puntaje = obtenerValor('PuntajeOriginal');
    var aceptacion = obtenerValor('AceptacionInstructor');

    if (!cursoId || !destrezaId || minutos === '' || segundos === '' || puntaje === '' || !aceptacion) {
        mostrarAlerta('Debe completar todos los campos obligatorios de la evaluación.');
        return false;
    }

    var minNum = parseInt(minutos, 10);
    var segNum = parseInt(segundos, 10);
    var puntajeNum = parseFloat(puntaje);

    if (isNaN(minNum) || minNum < 0) {
        mostrarAlerta('Los minutos deben ser 0 o mayores.');
        return false;
    }

    if (isNaN(segNum) || segNum < 0 || segNum > 59) {
        mostrarAlerta('Los segundos deben estar entre 0 y 59.');
        return false;
    }

    if (isNaN(puntajeNum) || puntajeNum < 1 || puntajeNum > 100) {
        mostrarAlerta('El puntaje debe estar entre 1 y 100.');
        return false;
    }

    var radios = document.querySelectorAll('#contenedorPuntosControl input[type="radio"]');
    if (radios.length > 0) {
        var grupos = {};
        for (var i = 0; i < radios.length; i++) {
            var radio = radios[i];
            if (!grupos[radio.name]) grupos[radio.name] = false;
            if (radio.checked) grupos[radio.name] = true;
        }

        for (var key in grupos) {
            if (Object.prototype.hasOwnProperty.call(grupos, key) && !grupos[key]) {
                mostrarAlerta('Debe indicar Cumplido o No Cumplido para todos los puntos críticos.');
                return false;
            }
        }
    }

    return true;
}

function cargarDestrezasAjax() {
    var cursoId = obtenerValor('CursoIdEvaluacion');
    var destrezaSelect = document.getElementById('DestrezaIdEvaluacion');

    if (!cursoId) {
        if (destrezaSelect) destrezaSelect.innerHTML = '<option value="">Seleccione destreza</option>';
        setHtml('contenedorPuntosControl', '<p>Seleccione una destreza para cargar sus puntos críticos.</p>');
        setTexto('cursoNombreTexto', '');
        setTexto('destrezaNombreTexto', '');
        return;
    }

    var cursoTexto = '';
    var cursoCombo = document.getElementById('CursoIdEvaluacion');
    if (cursoCombo && cursoCombo.selectedIndex >= 0) cursoTexto = cursoCombo.options[cursoCombo.selectedIndex].text;
    setTexto('cursoNombreTexto', cursoTexto);

    fetch('/Evaluaciones/DestrezasPorCurso?cursoId=' + encodeURIComponent(cursoId), {
        method: 'GET',
        headers: { 'X-Requested-With': 'XMLHttpRequest' }
    })
        .then(function (response) {
            if (!response.ok) throw new Error('HTTP ' + response.status);
            return response.json();
        })
        .then(function (result) {
            if (!result.ok) {
                mostrarAlerta(result.mensaje || 'No autorizado.');
                return;
            }

            if (destrezaSelect) {
                destrezaSelect.innerHTML = '<option value="">Seleccione destreza</option>';
                result.data.forEach(function (item) {
                    var opt = document.createElement('option');
                    opt.value = item.Id;
                    opt.text = item.Nombre;
                    destrezaSelect.appendChild(opt);
                });
            }

            setHtml('contenedorPuntosControl', '<p>Seleccione una destreza para cargar sus puntos críticos.</p>');
            setTexto('destrezaNombreTexto', '');
        })
        .catch(function (error) {
            console.error('Error cargarDestrezasAjax:', error);
            mostrarAlerta('Ocurrió un error al cargar las destrezas.');
        });
}

function cargarPuntosControlAjax() {
    var destrezaId = obtenerValor('DestrezaIdEvaluacion');

    if (!destrezaId) {
        setHtml('contenedorPuntosControl', '<p>Seleccione una destreza para cargar sus puntos críticos.</p>');
        setTexto('destrezaNombreTexto', '');
        return;
    }

    var destrezaTexto = '';
    var destrezaCombo = document.getElementById('DestrezaIdEvaluacion');
    if (destrezaCombo && destrezaCombo.selectedIndex >= 0) destrezaTexto = destrezaCombo.options[destrezaCombo.selectedIndex].text;
    setTexto('destrezaNombreTexto', destrezaTexto);

    fetch('/Evaluaciones/PuntosControlPorDestreza?destrezaId=' + encodeURIComponent(destrezaId), {
        method: 'GET',
        headers: { 'X-Requested-With': 'XMLHttpRequest' }
    })
        .then(function (response) {
            if (!response.ok) throw new Error('HTTP ' + response.status);
            return response.json();
        })
        .then(function (result) {
            if (!result.ok) {
                mostrarAlerta(result.mensaje || 'No se pudieron cargar los datos.');
                return;
            }

            var html = '';
            if (result.data && result.data.length > 0) {
                result.data.forEach(function (item, index) {
                    html += '<div class="critical-item">';
                    html += '<input type="hidden" name="PuntosControl[' + index + '].PuntoControlId" value="' + item.Id + '" />';
                    html += '<input type="hidden" name="PuntosControl[' + index + '].Descripcion" value="' + item.Descripcion + '" />';
                    html += '<div class="critical-title">' + item.Descripcion + '</div>';
                    html += '<div class="radio-group">';
                    html += '<label><input type="radio" name="PuntosControl[' + index + '].Cumplido" value="true" /> Cumplido</label>';
                    html += '<label><input type="radio" name="PuntosControl[' + index + '].Cumplido" value="false" /> No Cumplido</label>';
                    html += '</div>';
                    html += '</div>';
                });
            } else {
                html = '<p>No hay puntos de control configurados para esta destreza.</p>';
            }

            setHtml('contenedorPuntosControl', html);
        })
        .catch(function (error) {
            console.error('Error cargarPuntosControlAjax:', error);
            mostrarAlerta('Ocurrió un error al cargar los puntos de control.');
        });
}

// =============================
// MONITOREO
// =============================

function cargarMonitoreo() {
    var cohorteId = obtenerValor('cohorteId');
    var cursoId = obtenerValor('cursoId');

    if (!cohorteId || !cursoId) {
        mostrarAlerta('Debe seleccionar una cohorte y un curso.');
        return;
    }

    fetch('/Monitoreo/Resumen?cohorteId=' + encodeURIComponent(cohorteId) + '&cursoId=' + encodeURIComponent(cursoId), {
        method: 'GET',
        headers: { 'Accept': 'application/json' }
    })
        .then(function (response) {
            if (!response.ok) throw new Error('HTTP ' + response.status);
            return response.json();
        })
        .then(function (result) {
            if (!result.ok) {
                mostrarAlerta(result.mensaje || 'No autorizado.');
                return;
            }

            setTexto('tasaAprobacion', result.TasaGlobalAprobacion + '%');
            setTexto('estadoCertificacion', result.EstadoCertificacion);
            setTexto('intervencionPendiente', result.IntervencionPendiente);

            aplicarSemaforos(result);

            var html = '';
            if (result.Riesgos && result.Riesgos.length > 0) {
                html += '<ul class="risk-list">';
                result.Riesgos.forEach(function (r) {
                    html += '<li><a href="/Monitoreo/Riesgo?destrezaId=' + r.DestrezaId + '">' + r.Destreza + ' - ' + r.Cantidad + ' casos</a></li>';
                });
                html += '</ul>';
            } else {
                html = '<p>No hay áreas de riesgo registradas.</p>';
            }

            setHtml('riesgos', html);
            cargarIntervencionPendiente();
        })
        .catch(function (error) {
            console.error('Error cargarMonitoreo:', error);
            mostrarAlerta('Ocurrió un error al consultar el panel de monitoreo.');
        });
}

function cargarIntervencionPendiente() {
    var cohorteId = obtenerValor('cohorteId');
    var cursoId = obtenerValor('cursoId');

    fetch('/Monitoreo/IntervencionPendiente?cohorteId=' + encodeURIComponent(cohorteId) + '&cursoId=' + encodeURIComponent(cursoId), {
        method: 'GET',
        headers: { 'X-Requested-With': 'XMLHttpRequest' }
    })
        .then(function (response) {
            if (!response.ok) throw new Error('HTTP ' + response.status);
            return response.text();
        })
        .then(function (html) {
            setHtml('contenedorIntervencionPendiente', html);
        })
        .catch(function (error) {
            console.error('Error cargarIntervencionPendiente:', error);
            mostrarAlerta('Ocurrió un error al cargar la intervención pendiente.');
        });
}

function aplicarSemaforos(data) {
    var kpiAprobacion = document.getElementById('kpiAprobacion');
    var kpiCertificacion = document.getElementById('kpiCertificacion');
    var kpiPendiente = document.getElementById('kpiPendiente');

    if (kpiAprobacion) {
        if (data.TasaGlobalAprobacion >= 85) kpiAprobacion.className = 'kpi semaforo verde';
        else if (data.TasaGlobalAprobacion >= 70) kpiAprobacion.className = 'kpi semaforo amarillo';
        else kpiAprobacion.className = 'kpi semaforo rojo';
    }

    if (kpiCertificacion) {
        if (data.EstadoCertificacion > 0) kpiCertificacion.className = 'kpi semaforo verde';
        else kpiCertificacion.className = 'kpi semaforo amarillo';
    }

    if (kpiPendiente) {
        if (data.IntervencionPendiente === 0) kpiPendiente.className = 'kpi semaforo verde';
        else if (data.IntervencionPendiente <= 3) kpiPendiente.className = 'kpi semaforo amarillo';
        else kpiPendiente.className = 'kpi semaforo rojo';
    }
}

// =============================
// CATÁLOGOS
// =============================

function toggleArchivadoCohorteAjax(id) {
    console.log('Toggle cohorte:', id);

    postFormUrlEncoded('/Catalogos/ToggleArchivadoCohorteAjax', 'id=' + encodeURIComponent(id))
        .then(function (response) {
            if (!response.ok) throw new Error('HTTP ' + response.status);
            return response.json();
        })
        .then(function (data) {
            console.log('Respuesta cohorte:', data);
            mostrarAlerta(data.mensaje);
            if (data.ok) location.reload();
        })
        .catch(function (error) {
            console.error('Error toggleArchivadoCohorteAjax:', error);
            mostrarAlerta('Ocurrió un error al actualizar la cohorte.');
        });
}

function toggleArchivadoCursoAjax(id) {
    console.log('Toggle curso:', id);

    postFormUrlEncoded('/Catalogos/ToggleArchivadoCursoAjax', 'id=' + encodeURIComponent(id))
        .then(function (response) {
            if (!response.ok) throw new Error('HTTP ' + response.status);
            return response.json();
        })
        .then(function (data) {
            console.log('Respuesta curso:', data);
            mostrarAlerta(data.mensaje);
            if (data.ok) location.reload();
        })
        .catch(function (error) {
            console.error('Error toggleArchivadoCursoAjax:', error);
            mostrarAlerta('Ocurrió un error al actualizar el curso.');
        });
}

document.addEventListener('DOMContentLoaded', function () {
    console.log('app.js cargado correctamente');
});